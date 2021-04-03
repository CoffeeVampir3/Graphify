using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using GraphFramework.Attributes;
using GraphFramework.EditorOnly.Attributes;
using Direction = GraphFramework.Attributes.Direction;

namespace GraphFramework.Editor
{
    [Serializable]
    public class NodeModel : MovableModel
    {
        [SerializeReference] 
        protected internal RuntimeNode RuntimeData;
        [SerializeReference] 
        internal List<PortModel> portModels = new List<PortModel>();
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

        internal List<PortModel> AllPortModels()
        {
            List<PortModel> models = new List<PortModel>(portModels);
            foreach (var model in portModels)
            {
                if (model is DynamicPortModel dynPort)
                {
                    models.AddRange(dynPort.dynamicPorts);
                }
            }

            return models;
        }

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
            
            if (RuntimeData is SubgraphNode)
            {
                CreateSubgraphNode(graphModel);
            }
            RuntimeData.name = nodeTitle;
            AssetDatabase.AddObjectToAsset(RuntimeData, graphModel);
            EditorUtility.SetDirty(graphModel);
            
            graphModel.serializedGraphBlueprint.nodes.Add(RuntimeData);
        }
        
        /// <summary>
        /// Deep copies another nodes runtime data onto this node.
        /// </summary>
        public void CreateRuntimeDataClone(GraphModel graphModel, RuntimeNode toCopy)
        {
            RuntimeData = ScriptableObject.Instantiate(toCopy);
            AssetDatabase.AddObjectToAsset(RuntimeData, graphModel);
            EditorUtility.SetDirty(graphModel);
            
            graphModel.serializedGraphBlueprint.nodes.Add(RuntimeData);
        }

        public void CreateSubgraphNode(GraphModel graphModel)
        {
            var sn = RuntimeData as SubgraphNode;
            sn.parentBpGuid = graphModel.serializedGraphBlueprint.AssetGuid;

            var parentBlueprint = GraphModel.GetBlueprintByGuid(graphModel, sn.parentBpGuid);
            var parentModel = GraphModel.GetModelFromBlueprint(parentBlueprint);
            
            if (parentModel == null) return;
            var child = parentModel.CreateChildGraph(graphModel , 
                this, sn, graphModel.serializedGraphBlueprint.GetType());
            
            SerializedObject so = new SerializedObject(sn);
            so.FindProperty(nameof(sn.childBpGuid)).stringValue = child.serializedGraphBlueprint.AssetGuid;
            so.ApplyModifiedProperties();

            SerializedObject on = new SerializedObject(child.rootNodeModel.RuntimeData);
            on.FindProperty(nameof(sn.childBpGuid)).stringValue = graphModel.serializedGraphBlueprint.AssetGuid;
            on.FindProperty(nameof(sn.childBpGuid)).stringValue = child.serializedGraphBlueprint.AssetGuid;
            on.ApplyModifiedProperties();
        }

        public void DeleteSubgraphNode(GraphModel graphModel)
        {
            var sn = RuntimeData as SubgraphNode;
            if (sn == null)
                return;
            
            var childBlueprint = GraphModel.GetBlueprintByGuid(graphModel, sn.childBpGuid);
            var model = GraphModel.GetModelFromBlueprint(childBlueprint);
            graphModel.DeleteChildGraph(model);
            var parentBp = GraphModel.GetBlueprintByGuid(graphModel, sn.parentBpGuid);
            parentBp.nodes.Remove(sn.childNode);
            
            Undo.DestroyObjectImmediate(model);
            Undo.DestroyObjectImmediate(childBlueprint);
        }

        public void Delete(GraphModel graphModel)
        {
            foreach (var model in portModels)
            {
                if (model is DynamicPortModel dynPort)
                {
                    dynPort.DeleteAllLinks();
                }
            }

            if (RuntimeData is SubgraphNode)
            {
                DeleteSubgraphNode(graphModel);
            }
            graphModel.serializedGraphBlueprint.nodes.Remove(RuntimeData);
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

        protected internal static readonly Dictionary<Type, bool> changedTypeCache = 
            new Dictionary<Type, bool>();
        protected internal static readonly Dictionary<Type, HashSet<string>> existingFieldNamesCache =
            new Dictionary<Type, HashSet<string>>();
        protected internal static readonly Dictionary<Type, HashSet<int>> changedFieldIndexCache =
            new Dictionary<Type, HashSet<int>>();

        public static void ClearChangeTrackingCache()
        {
            changedTypeCache.Clear();
            existingFieldNamesCache.Clear();
            changedFieldIndexCache.Clear();
        }

        protected internal bool TrackChanges(ref HashSet<string> changedFieldNames)
        {
            if (changedTypeCache.TryGetValue(RuntimeData.GetType(), out var changed))
            {
                if (!changed)
                    return false;
                
                var cachedFieldNames = existingFieldNamesCache[RuntimeData.GetType()];
                var changedIndices = changedFieldIndexCache[RuntimeData.GetType()];
                for (int i = portModels.Count - 1; i >= 0; i--)
                {
                    if (changedIndices.Contains(i))
                    {
                        portModels.RemoveAt(i);
                    }
                }
                CreatePortModelsFromReflection(false, cachedFieldNames);
                return true;
            }

            HashSet<string> actualFieldNames = new HashSet<string>();
            HashSet<string> existingFieldNames = new HashSet<string>();
            HashSet<int> changedFieldIndices = new HashSet<int>();
            bool anyChanges = false;
            
            var fieldInfo = GetFieldInfoFor(RuntimeData.GetType());
            Dictionary<string, Type> stringToType = new Dictionary<string, Type>();
            foreach (var info in fieldInfo.fieldInfo)
            {
                actualFieldNames.Add(info.Name);
                stringToType.Add(info.Name, info.FieldType);
            }
            
            for (int i = portModels.Count - 1; i >= 0; i--)
            {
                PortModel port = portModels[i];
                if (!actualFieldNames.Contains(port.serializedValueFieldInfo.FieldName) ||
                    !stringToType.TryGetValue(port.serializedValueFieldInfo.FieldName, out var newType) ||
                    port.portCompleteType.type != newType)
                {
                    anyChanges = true;
                    changedFieldNames.Add(port.serializedValueFieldInfo.FieldName);
                    changedFieldIndices.Add(i);
                    portModels.RemoveAt(i);
                    continue;
                }

                existingFieldNames.Add(port.serializedValueFieldInfo.FieldName);
            }

            changedTypeCache[RuntimeData.GetType()] = anyChanges;

            if (!anyChanges) 
                return false;
            
            existingFieldNamesCache[RuntimeData.GetType()] = existingFieldNames;
            changedFieldIndexCache[RuntimeData.GetType()] = changedFieldIndices;
            CreatePortModelsFromReflection(false, existingFieldNames);
            return true;
        }

        protected internal UnityEditor.Experimental.GraphView.Port.Capacity CapacityToUnity(Capacity cap)
        {
            if (cap == Capacity.Single)
            {
                return Port.Capacity.Single;
            }

            return Port.Capacity.Multi;
        }

        protected internal PortModel CreatePortModel(FieldInfo field, Direction dir, Capacity cap, HashSet<string> fieldNames)
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
                return null;
            }

            if (fieldNames != null && fieldNames.Contains(field.Name))
            {
                //This field previously existed and did not change.
                return null;
            }

            var portValueType = field.FieldType.GetGenericArguments();
            //If we add more port types this should become a factory.
            if (typeof(DynamicValuePort).IsAssignableFrom(field.FieldType))
            {
                var dynamicRange = field.GetCustomAttribute<DynamicRange>();
                var dynModel = new DynamicPortModel(Orientation.Horizontal, unityDirection, 
                    CapacityToUnity(cap), portValueType.FirstOrDefault(), field);
                if (dynamicRange != null)
                {
                    dynModel.minSize = dynamicRange.min;
                    dynModel.maxSize = dynamicRange.max;
                }
                portCreationAction.Invoke(dynModel);
                return dynModel;
            }
            var pm = new PortModel(Orientation.Horizontal, unityDirection, 
                CapacityToUnity(cap), portValueType.FirstOrDefault(),field);
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
        protected internal void CreatePortModelsFromReflection(bool clearLinks = false, HashSet<string> fieldNames = null)
        {
            var fieldsAndData = GetFieldInfoFor(RuntimeData.GetType());
            
            for (int i = fieldsAndData.fieldInfo.Count - 1; i >= 0; i--)
            {
                var field = fieldsAndData.fieldInfo[i];
                var cap = fieldsAndData.caps[i];
                var dir = fieldsAndData.directions[i];
                var portModel = CreatePortModel(field, dir, cap, fieldNames);
                if (!clearLinks || portModel == null) continue;
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