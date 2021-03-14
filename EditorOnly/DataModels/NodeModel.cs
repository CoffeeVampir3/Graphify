using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using GraphFramework.Attributes;

namespace GraphFramework.Editor
{
    [Serializable]
    public class NodeModel : MovableModel
    {
        [SerializeReference]
        protected internal RuntimeNode RuntimeData;
        [SerializeReference]
        protected internal List<PortModel> inputPorts = new List<PortModel>();
        [SerializeReference]
        protected internal List<PortModel> outputPorts = new List<PortModel>();
        //TODO:: Finish dyn ports.
        [SerializeReference]
        protected internal List<PortModel> dynamicInputPorts = new List<PortModel>();
        [SerializeReference]
        protected internal List<PortModel> dynamicOutputPorts = new List<PortModel>();
        [SerializeReference] 
        protected internal StackModel stackedOn = null;
        [SerializeReference] 
        protected internal bool isExpanded = true;
        [SerializeField] 
        private string nodeTitle = "Untitled.";
        [SerializeField] 
        private Rect position = Rect.zero;

        [NonSerialized] 
        private NodeView view;

        public NodeView View => view;

        #region Creation & Cloning
        
        public static NodeModel InstantiateModel(string initialName, GraphModel graphModel, Type runtimeDataType)
        {
            var model = new NodeModel {nodeTitle = initialName};
            model.CreateRuntimeData(graphModel, runtimeDataType);
            model.CreatePortModelsFromReflection();
            
            return model;
        }

        public NodeModel Clone(GraphModel graphModel)
        {
            NodeModel model = new NodeModel
            {
                nodeTitle = nodeTitle, 
                position = position, 
                isExpanded = isExpanded
            };
            model.CreateRuntimeDataClone(graphModel, RuntimeData);
            model.RuntimeData.name = RuntimeData.name;
            model.CreatePortModelsFromReflection(true);
            return model;
        }

        public void CreateRuntimeData(GraphModel graphModel, Type runtimeDataType)
        {
            RuntimeData = ScriptableObject.CreateInstance(runtimeDataType) as RuntimeNode;
            
            Debug.Assert(RuntimeData != null, nameof(RuntimeData) + " runtime data type was somehow null, something horrible has happened please report a bug.");
            RuntimeData.name = nodeTitle;
            AssetDatabase.AddObjectToAsset(RuntimeData, graphModel);
            EditorUtility.SetDirty(graphModel);
        }
        
        public void CreateRuntimeDataClone(GraphModel graphModel, RuntimeNode toCopy)
        {
            RuntimeData = ScriptableObject.Instantiate(toCopy);
            AssetDatabase.AddObjectToAsset(RuntimeData, graphModel);
            EditorUtility.SetDirty(graphModel);
        }

        #endregion
        
        #region Ports

        protected internal void CreatePortModel(FieldInfo field, Direction dir)
        {
            Action<PortModel> portCreationAction = null;
            Action<PortModel> dynamicPortCreationAction = null;
            switch (dir)
            {
                case Direction.Input:
                    portCreationAction = inputPorts.Add;
                    dynamicPortCreationAction = dynamicInputPorts.Add;
                    break;
                case Direction.Output:
                    portCreationAction = outputPorts.Add;
                    dynamicPortCreationAction = dynamicOutputPorts.Add;
                    break;
            }
            
            //This extracts the <T> type from the given ValuePort<T>.
            if (typeof(ValuePort).IsAssignableFrom(field.FieldType))
            {
                var portValueType = field.FieldType.
                    GetGenericClassConstructorArguments(typeof(ValuePort<>));
                var pm = new PortModel(Orientation.Horizontal, dir, 
                    Port.Capacity.Multi, field.FieldType, 
                    portValueType.FirstOrDefault(), field);
                portCreationAction?.Invoke(pm);
                return;
            }

            //This extracts the <T> type from the given DynamicValuePort<T>.
            if (typeof(DynamicValuePort).IsAssignableFrom(field.FieldType))
            {
                var portValueType = field.FieldType.
                    GetGenericClassConstructorArguments(typeof(DynamicValuePort<>));
                var pm = new PortModel(Orientation.Horizontal, dir, 
                    Port.Capacity.Multi, field.FieldType, 
                    portValueType.FirstOrDefault(), field);
                dynamicPortCreationAction?.Invoke(pm);
            }
        }

        //(typeof(RuntimeData), typeof(DirectionAttribute))
        //Reflection data cache for (node, dir), List of ports to generate.
        private static readonly Dictionary<(Type, Type), List<FieldInfo>> dataTypeToInfoFields
            = new Dictionary<(Type, Type), List<FieldInfo>>();

        /// <summary>
        /// Cached wrapper around GetLocalFieldWithAttribute
        /// </summary>
        private List<FieldInfo> GetFieldInfoFor<Attr>(Type runtimeDataType)
            where Attr : Attribute
        {
            var index = (runtimeDataType, typeof(Attr));
            if (dataTypeToInfoFields.TryGetValue(index, out var fields))
            {
                return fields;
            }

            fields = runtimeDataType.GetLocalFieldsWithAttribute<Attr>();
            dataTypeToInfoFields.Add(index, fields);
            return fields;
        }
        
        /// <summary>
        /// Analyses the reflection data and creates the appropriate ports based on it automagically.
        /// </summary>
        /// <param name="clearCopy">Copied models should use true, clears all port links if true.</param>
        protected internal void CreatePortModelsFromReflection(bool clearCopy = false)
        {
            var oFields = GetFieldInfoFor<Out>(RuntimeData.GetType());
            var iFields = GetFieldInfoFor<In>(RuntimeData.GetType());

            foreach (var field in iFields)
            {
                CreatePortModel(field, Direction.Input);
                if (!clearCopy) continue;
                if (field.GetValue(RuntimeData) is ValuePort vp)
                {
                    vp.links.Clear();
                }
            }
            foreach (var field in oFields)
            {
                CreatePortModel(field, Direction.Output);
                if (!clearCopy) continue;
                if (field.GetValue(RuntimeData) is ValuePort vp)
                {
                    vp.links.Clear();
                }
            }
        }
        
        #endregion
        
        #region Data Model Controller

        /// <summary>
        /// Creates a NodeView from this model's data.
        /// </summary>
        public NodeView CreateView()
        {
            view = new NodeView(this);
            view.Display();
            return view;
        }

        public void UpdatePosition()
        {
            position = view.GetPosition();
        }
        
        public string NodeTitle
        {
            get => nodeTitle;

            set
            {
                nodeTitle = value;
                view?.OnDirty();
            }
        }

        public Rect Position
        {
            get => position;
            set
            {
                position = value;
                view?.OnDirty();
            }
        }

        #endregion
        
        #region Connections

        [NonSerialized]
        private Dictionary<PortModel, RuntimePort> cachedValuePorts = 
            new Dictionary<PortModel, RuntimePort>();
        private bool TryResolveValuePortFromModels(PortModel portModel,
            out RuntimePort valuePort)
        {
            if (cachedValuePorts.TryGetValue(portModel, out valuePort))
            {
                return true;
            }
            try
            {
                var inputPortInfo = portModel.serializedValueFieldInfo.FieldFromInfo;
                valuePort = inputPortInfo.GetValue(RuntimeData) as ValuePort;
                cachedValuePorts.Add(portModel, valuePort);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if a link can be made to the given OutputModel's OutputPort.
        /// </summary>
        public bool CanPortLinkTo(PortModel myInputPort, 
            NodeModel outputModel, PortModel outputPort)
        {
            return TryResolveValuePortFromModels(myInputPort, out _) 
                   && outputModel.TryResolveValuePortFromModels(outputPort, out _);
        }

        /// <summary>
        /// Deletes the link on the given port via GUID.
        /// </summary>
        public void DeletePortLinkByGuid(PortModel inPort, string guid)
        {
            if (!TryResolveValuePortFromModels(inPort, out var runtimePort))
                return;

            switch (runtimePort)
            {
                case ValuePort valuePort:
                    for (int i = valuePort.links.Count - 1; i >= 0; i--)
                    {
                        Link currentLink = valuePort.links[i];
                        if (currentLink.GUID != guid) continue;
                        valuePort.links.Remove(currentLink);
                        return;
                    }
                    break;
                case DynamicValuePort dynamicVp:
                    throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// Creates a link between this node's given input port and the target NodeModel's
        /// output port.
        /// </summary>
        /// <returns>The created link</returns>
        public Link LinkPortTo(PortModel myInputPort, NodeModel outputModel, PortModel outputPort)
        {
            var localConnection = new Link(RuntimeData, myInputPort.serializedValueFieldInfo,
                outputModel.RuntimeData, outputPort.serializedValueFieldInfo);

            //Guaranteed to be cached if CanPortConnectTo returned true.
            var inputRuntimePort = cachedValuePorts[myInputPort];
            switch (inputRuntimePort)
            {
                case ValuePort inputValuePort:
                    for (int i = inputValuePort.links.Count - 1; i >= 0; i--)
                    {
                        inputValuePort.links.Add(localConnection);
                    }
                    break;
                case DynamicValuePort dynamicVp:
                    throw new System.NotImplementedException();
            }
            return localConnection;
        }
        
        #endregion
    }
}