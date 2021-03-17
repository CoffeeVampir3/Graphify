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

        protected internal void CreatePortModel(FieldInfo field, Direction dir, Port.Capacity cap)
        {
            Action<PortModel> portCreationAction;
            switch (dir)
            {
                case Direction.Input:
                    portCreationAction = inputPorts.Add;
                    break;
                case Direction.Output:
                    portCreationAction = outputPorts.Add;
                    break;
                default:
                    return;
            }
            
            var portValueType = field.FieldType.
                GetGenericClassConstructorArguments(typeof(ValuePort<>));
            var pm = new PortModel(Orientation.Horizontal, dir, 
                cap, portValueType.FirstOrDefault(), field);
            portCreationAction.Invoke(pm);
        }

        //(typeof(RuntimeData), typeof(DirectionAttribute))
        //Reflection data cache for (node, dir), List of PortInfo and list of "PortMetadata"
        private static readonly Dictionary<(Type, Type), PortInfoAndMetadata> dataTypeToInfoFields
            = new Dictionary<(Type, Type), PortInfoAndMetadata>();
        
        private readonly struct PortInfoAndMetadata
        {
            public readonly List<FieldInfo> fieldInfo;
            public readonly List<Port.Capacity> caps;

            public PortInfoAndMetadata(List<FieldInfo> fieldInfo, List<Port.Capacity> caps)
            {
                this.fieldInfo = fieldInfo;
                this.caps = caps;
            }
        }

        /// <summary>
        /// Scans the attribute for our ports and returns the field and metadata.
        /// </summary>
        private PortInfoAndMetadata GetFieldInfoFor<Attr>(Type runtimeDataType)
            where Attr : Attribute
        {
            var index = (runtimeDataType, typeof(Attr));
            if (dataTypeToInfoFields.TryGetValue(index, out var info))
            {
                return info;
            }

            var fields = runtimeDataType.GetLocalFieldsWithAttribute<Attr>(out var attribs);
            List<Port.Capacity> caps = new List<Port.Capacity>();
            foreach (var attr in attribs)
            {
                if (attr is In inAttr)
                {
                    caps.Add(inAttr.capacity);
                } 
                if (attr is Out outAttr)
                {
                    caps.Add(outAttr.capacity);
                }
            }
            
            PortInfoAndMetadata iacp = new PortInfoAndMetadata(fields, caps);
            dataTypeToInfoFields.Add(index, iacp);
            return iacp;
        }

        /// <summary>
        /// Analyses the reflection data and creates the appropriate ports based on it automagically.
        /// </summary>
        /// <param name="clearCopy">Copied models should use true, clears all port links if true.</param>
        protected internal void CreatePortModelsFromReflection(bool clearCopy = false)
        {
            var oFieldAndCaps = GetFieldInfoFor<Out>(RuntimeData.GetType());
            var iFieldAndCaps = GetFieldInfoFor<In>(RuntimeData.GetType());

            for (int i = 0; i < iFieldAndCaps.fieldInfo.Count; i++)
            {
                var field = iFieldAndCaps.fieldInfo[i];
                var cap = iFieldAndCaps.caps[i];
                CreatePortModel(field, Direction.Input, cap);
                if (!clearCopy) continue;
                if (field.GetValue(RuntimeData) is ValuePort vp)
                {
                    vp.links.Clear();
                }
            }
            
            for (int i = 0; i < oFieldAndCaps.fieldInfo.Count; i++)
            {
                var field = oFieldAndCaps.fieldInfo[i];
                var cap = oFieldAndCaps.caps[i];
                CreatePortModel(field, Direction.Output, cap);
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
        private Dictionary<PortModel, ValuePort> cachedValuePorts = 
            new Dictionary<PortModel, ValuePort>();
        private bool TryResolveValuePortFromModels(PortModel portModel,
            out ValuePort valuePort)
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
            if (!TryResolveValuePortFromModels(inPort, out var valuePort))
                return;

            for (int i = valuePort.links.Count - 1; i >= 0; i--)
            {
                Link currentLink = valuePort.links[i];
                if (currentLink.GUID != guid) continue;
                valuePort.links.Remove(currentLink);
                return;
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
            var inputValuePort = cachedValuePorts[myInputPort];
            inputValuePort.links.Add(localConnection);
            return localConnection;
        }
        
        #endregion
    }
}