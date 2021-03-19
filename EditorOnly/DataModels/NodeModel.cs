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
        [field: NonSerialized]
        public NodeView View { get; private set; }

        #region Creation & Cloning
        
        public static NodeModel InstantiateModel(string initialName, GraphModel graphModel, Type runtimeDataType)
        {
            var model = new NodeModel {nodeTitle = initialName};
            model.CreateRuntimeData(graphModel, runtimeDataType);
            model.CreatePortModelsFromReflection();
            
            return model;
        }

        /// <summary>
        /// Creates a deep-copy of this node model with it's own unique references.
        /// </summary>
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
        
        /// <summary>
        /// Deep copies another nodes runtime data onto this node.
        /// </summary>
        public void CreateRuntimeDataClone(GraphModel graphModel, RuntimeNode toCopy)
        {
            RuntimeData = ScriptableObject.Instantiate(toCopy);
            AssetDatabase.AddObjectToAsset(RuntimeData, graphModel);
            EditorUtility.SetDirty(graphModel);
        }

        #endregion
        
        #region Ports

        protected internal void UpdatePorts()
        {
            var fieldsAndData = GetFieldInfoFor(RuntimeData.GetType());
            HashSet<string> portNames = new HashSet<string>();
            HashSet<string> fieldnames = new HashSet<string>();

            foreach (var port in inputPorts)
            {
                portNames.Add(port.serializedValueFieldInfo.FieldName);
            }

            foreach (var port in outputPorts)
            {
                portNames.Add(port.serializedValueFieldInfo.FieldName);
            }

            //Add new.
            for (int i = 0; i < fieldsAndData.fieldInfo.Count; i++)
            {
                var field = fieldsAndData.fieldInfo[i];
                var cap = fieldsAndData.caps[i];
                var dir = fieldsAndData.directions[i];
                fieldnames.Add(field.Name);
                
                if(!portNames.Contains(field.Name)) {
                    CreatePortModel(field, dir, cap);
                }
            }

            //Remove old
            for (int i = inputPorts.Count - 1; i >= 0; i--)
            {
                var port = inputPorts[i];
                if (!fieldnames.Contains(port.serializedValueFieldInfo.FieldName))
                {
                    inputPorts.Remove(port);
                }
            }
            for (int i = outputPorts.Count - 1; i >= 0; i--)
            {
                var port = outputPorts[i];
                if (!fieldnames.Contains(port.serializedValueFieldInfo.FieldName))
                {
                    outputPorts.Remove(port);
                }
            }
        }

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

        private readonly struct PortInfoAndMetadata
        {
            public readonly List<FieldInfo> fieldInfo;
            public readonly List<Port.Capacity> caps;
            public readonly List<Direction> directions;

            public PortInfoAndMetadata(List<FieldInfo> fieldInfo, List<Port.Capacity> caps, 
                List<Direction> dirs)
            {
                this.fieldInfo = fieldInfo;
                this.directions = dirs; 
                this.caps = caps;
            }
        }

        private static Dictionary<Type, PortInfoAndMetadata> cachedFieldInfo =
            new Dictionary<Type, PortInfoAndMetadata>();

        /// <summary>
        /// Scans the attribute for our ports and returns the field and metadata.
        /// </summary>
        private PortInfoAndMetadata GetFieldInfoFor(Type runtimeDataType)
        {
            if (cachedFieldInfo.TryGetValue(runtimeDataType, out var info))
                return info;
            
            var fields = runtimeDataType.GetLocalFieldsWithAttribute<DirectionalAttribute>(out var attribs);
            
            List<Port.Capacity> caps = new List<Port.Capacity>();
            List<Direction> directions = new List<Direction>();
            foreach (var attr in attribs)
            {
                if (!(attr is DirectionalAttribute dAttr)) continue;
                caps.Add(dAttr.capacity);
                directions.Add(dAttr.direction);
            }
            
            PortInfoAndMetadata pInfo = new PortInfoAndMetadata(fields, caps, directions);
            cachedFieldInfo.Add(runtimeDataType, pInfo);
            return pInfo;
        }

        /// <summary>
        /// Analyses the reflection data and creates the appropriate ports based on it automagically.
        /// </summary>
        /// <param name="clearCopy">Copied models should use true, clears all port links if true.</param>
        protected internal void CreatePortModelsFromReflection(bool clearCopy = false)
        {
            var fieldsAndData = GetFieldInfoFor(RuntimeData.GetType());
            
            for (int i = 0; i < fieldsAndData.fieldInfo.Count; i++)
            {
                var field = fieldsAndData.fieldInfo[i];
                var cap = fieldsAndData.caps[i];
                var dir = fieldsAndData.directions[i];
                CreatePortModel(field, dir, cap);
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
            View = new NodeView(this);
            View.Display();
            return View;
        }

        public void UpdatePosition()
        {
            position = View.GetPosition();
        }
        
        public string NodeTitle
        {
            get => nodeTitle;

            set
            {
                nodeTitle = value;
                View?.OnDirty();
            }
        }

        public Rect Position
        {
            get => position;
            set
            {
                position = value;
                View?.OnDirty();
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
                Debug.LogError("Attempted to resolve a node's value port relation for " + this.nodeTitle + " but the field " + portModel.serializedValueFieldInfo.FieldName + " was not found!");
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