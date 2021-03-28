using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using GraphFramework.Attributes;
using Direction = GraphFramework.Attributes.Direction;

namespace GraphFramework.Editor
{
    [Serializable]
    public class NodeModel : MovableModel
    {
        [SerializeReference]
        protected internal RuntimeNode RuntimeData;
        [SerializeReference]
        protected internal List<PortModel> portModels = new List<PortModel>();
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

        protected void ClearPort(PortModel model)
        {
            FieldInfo field = model.serializedValueFieldInfo.FieldFromInfo;
            if (field.GetValue(RuntimeData) is BasePort vp)
            {
                vp.Clear();
            }
        }
        
        #region Change Tracking
        
        private readonly Dictionary<string, string> fieldNameToOldGuid = 
            new Dictionary<string, string>();
        private static readonly Dictionary<Type, bool> filthyPortsDictionary = 
            new Dictionary<Type, bool>();

        protected internal static void PreGraphBuild()
        {
            filthyPortsDictionary.Clear();
        }
        protected internal void UpdatePorts()
        {
            //Fast path, we examine cached changes so this doesn't take an eternity.
            if (filthyPortsDictionary.TryGetValue(RuntimeData.GetType(), out var shouldChange))
            {
                if (!shouldChange) return;
                fieldNameToOldGuid.Clear();
                for (int i = portModels.Count - 1; i >= 0; i--)
                {
                    PortModel port = portModels[i];
                    fieldNameToOldGuid.Add(port.serializedValueFieldInfo.FieldName, port.portGUID);
                }
                portModels.Clear();
                CreatePortModelsFromReflection(false, true);
                return;
            }
            
            var fieldInfo = GetFieldInfoFor(RuntimeData.GetType());
            HashSet<string> fieldNames = new HashSet<string>();
            bool anyChanges = false;
            
            foreach (var info in fieldInfo.fieldInfo)
            {
                fieldNames.Add(info.Name);
            }
            fieldNameToOldGuid.Clear();
            for (int i = portModels.Count - 1; i >= 0; i--)
            {
                PortModel port = portModels[i];
                fieldNameToOldGuid.Add(port.serializedValueFieldInfo.FieldName, port.portGUID);
                //Field removed or renamed
                if (!fieldNames.Contains(port.serializedValueFieldInfo.FieldName))
                {
                    anyChanges = true;
                    break;
                }
            }

            //Field added
            anyChanges |= fieldInfo.fieldInfo.Count != portModels.Count;
            filthyPortsDictionary.Add(RuntimeData.GetType(), anyChanges);

            if (!anyChanges) return;
            portModels.Clear();
            CreatePortModelsFromReflection(false, true);
        }
        
        #endregion

        protected internal UnityEditor.Experimental.GraphView.Port.Capacity CapacityToUnity(Capacity cap)
        {
            if (cap == Capacity.Single)
            {
                return Port.Capacity.Single;
            }

            return Port.Capacity.Multi;
        }

        protected internal PortModel CreatePortModel(FieldInfo field, Direction dir, Capacity cap, bool update)
        {
            Action<PortModel> portCreationAction;
            UnityEditor.Experimental.GraphView.Direction unityDirection;
            if (dir == Direction.Input)
            {
                portCreationAction = portModels.Add;
                unityDirection = UnityEditor.Experimental.GraphView.Direction.Input;
            }
            else
            {
                portCreationAction = portModels.Add;
                unityDirection = UnityEditor.Experimental.GraphView.Direction.Output;
            }

            if (!typeof(BasePort).IsAssignableFrom(field.FieldType))
            {
                Debug.LogError("Attempted to construct port that is not assignable to value port.");
            }
            
            if (update && fieldNameToOldGuid.TryGetValue(field.Name, out var guid))
            {
                //Empty
            }
            else
            {
                guid = Guid.NewGuid().ToString();
            }
            
            var portValueType = field.FieldType.GetGenericArguments();
            //If we add more port types this should become a factory.
            if (typeof(DynamicValuePort).IsAssignableFrom(field.FieldType))
            {
                var dynModel = new DynamicPortModel(Orientation.Horizontal, unityDirection, 
                    CapacityToUnity(cap), portValueType.FirstOrDefault(), field, guid);
                portCreationAction.Invoke(dynModel);
                return dynModel;
            }
            var pm = new PortModel(Orientation.Horizontal, unityDirection, 
                CapacityToUnity(cap), portValueType.FirstOrDefault(), field, guid);
            portCreationAction.Invoke(pm);
            return pm;
        }

        private readonly struct PortInfoAndMetadata
        {
            public readonly List<FieldInfo> fieldInfo;
            public readonly List<Capacity> caps;
            public readonly List<Direction> directions;

            public PortInfoAndMetadata(List<FieldInfo> fieldInfo, List<Capacity> caps, 
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
            
            List<Capacity> caps = new List<Capacity>();
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
        /// <param name="clearLinks">Copied models should use true, clears all port links if true.</param>
        protected internal void CreatePortModelsFromReflection(bool clearLinks = false, bool update = false)
        {
            var fieldsAndData = GetFieldInfoFor(RuntimeData.GetType());
            
            for (int i = 0; i < fieldsAndData.fieldInfo.Count; i++)
            {
                var field = fieldsAndData.fieldInfo[i];
                var cap = fieldsAndData.caps[i];
                var dir = fieldsAndData.directions[i];
                var portModel = CreatePortModel(field, dir, cap, update);
                if (!clearLinks) continue;
                ClearPort(portModel);
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
        private Dictionary<PortModel, BasePort> cachedValuePorts = 
            new Dictionary<PortModel, BasePort>();
        private bool TryResolveValuePortFromModels(PortModel portModel,
            out BasePort valuePort)
        {
            if (cachedValuePorts.TryGetValue(portModel, out valuePort))
            {
                return true;
            }
            try
            {
                var inputPortInfo = portModel.serializedValueFieldInfo.FieldFromInfo;
                valuePort = inputPortInfo.GetValue(RuntimeData) as BasePort;
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
                inPort.linkGuids.Remove(guid);
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
            var localConnection = new Link(RuntimeData, 
                myInputPort.serializedValueFieldInfo,
                myInputPort.dynamicIndex,
                outputModel.RuntimeData, 
                outputPort.serializedValueFieldInfo,
                outputPort.dynamicIndex);

            myInputPort.linkGuids.Add(localConnection.GUID);
            
            //Guaranteed to be cached if CanPortConnectTo returned true.
            var inputValuePort = cachedValuePorts[myInputPort];
            inputValuePort.links.Add(localConnection);
            return localConnection;
        }
        
        #endregion
    }
}