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
        public RuntimeNode RuntimeData;
        [SerializeReference]
        public List<PortModel> inputPorts = new List<PortModel>();
        [SerializeReference]
        public List<PortModel> outputPorts = new List<PortModel>();
        //This makes stacking things really really easy.
        [SerializeReference] 
        public StackModel stackedOn = null;
        
        [SerializeField] 
        public bool isRoot = false;
        [SerializeField] 
        private string nodeTitle = "Untitled.";
        [SerializeField] 
        private Rect position = Rect.zero;
        [SerializeField] 
        private bool isExpanded = true;
        
        [NonSerialized] 
        private NodeView view;

        public NodeView View => view;

        #region Creation & Cloning
        
        public static NodeModel InstantiateModel(BetaEditorGraph editorGraph, Type runtimeDataType)
        {
            var model = new NodeModel();
            model.CreateRuntimeData(editorGraph, runtimeDataType);
            model.CreatePortModels();
            return model;
        }

        public NodeModel Clone(BetaEditorGraph editorGraph)
        {
            NodeModel model = new NodeModel
            {
                isRoot = false, 
                nodeTitle = nodeTitle, 
                position = position, 
                isExpanded = isExpanded
            };
            model.CreateRuntimeDataClone(editorGraph, RuntimeData);
            model.RuntimeData.name = RuntimeData.name;
            model.CreatePortModels(true);
            return model;
        }

        public void CreateRuntimeData(BetaEditorGraph editorGraph, Type runtimeDataType)
        {
            RuntimeData = ScriptableObject.CreateInstance(runtimeDataType) as RuntimeNode;
            AssetDatabase.AddObjectToAsset(RuntimeData, editorGraph);
            EditorUtility.SetDirty(editorGraph);
        }
        
        public void CreateRuntimeDataClone(BetaEditorGraph editorGraph, RuntimeNode toCopy)
        {
            RuntimeData = ScriptableObject.Instantiate(toCopy);
            AssetDatabase.AddObjectToAsset(RuntimeData, editorGraph);
            EditorUtility.SetDirty(editorGraph);
        }

        public PortModel CreatePortModel(FieldInfo field, Direction dir)
        {
            var portValueType = field.FieldType.
                GetGenericClassConstructorArguments(typeof(ValuePort<>));
            return new PortModel(Orientation.Horizontal, dir, 
                Port.Capacity.Multi, portValueType.FirstOrDefault(), field);
        }

        public void CreatePortModels(bool clearCopy = false)
        {
            var oFields = RuntimeData.GetType().GetLocalFieldsWithAttribute<Out>();
            var iFields = RuntimeData.GetType().GetLocalFieldsWithAttribute<In>();

            foreach (var field in iFields)
            {
                PortModel p = CreatePortModel(field, Direction.Input);
                inputPorts.Add(p);
                if (!clearCopy) continue;
                if (field.GetValue(RuntimeData) is ValuePort vp)
                {
                    vp.links.Clear();
                }
            }
            foreach (var field in oFields)
            {
                PortModel p = CreatePortModel(field, Direction.Output);
                outputPorts.Add(p);
                if (!clearCopy) continue;
                if (field.GetValue(RuntimeData) is ValuePort vp)
                {
                    vp.links.Clear();
                }
            }
        }
        
        #endregion
        
        #region Data Model Controller

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

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
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

        public bool CanPortConnectTo(PortModel myInputPort, 
            NodeModel outputModel, PortModel outputPort)
        {
            return TryResolveValuePortFromModels(myInputPort, out _) 
                   && outputModel.TryResolveValuePortFromModels(outputPort, out _);
        }

        public void DeletePortConnectionByGuid(PortModel inPort, string guid)
        {
            if (!TryResolveValuePortFromModels(inPort, out var valuePort))
                return;
            //Delete value port connection
            for (int i = valuePort.links.Count - 1; i >= 0; i--)
            {
                Link currentLink = valuePort.links[i];
                if (currentLink.GUID != guid) continue;
                valuePort.links.Remove(currentLink);
                return;
            }
        }

        public Link ConnectPortTo(PortModel myInputPort, NodeModel outputModel, PortModel outputPort)
        {
            var localConnection = new Link(RuntimeData, myInputPort.serializedValueFieldInfo,
                outputModel.RuntimeData, outputPort.serializedValueFieldInfo);

            //Guaranteed to be cached if CanPortConnectTo returned true.
            var inputValuePort = cachedValuePorts[myInputPort];
            inputValuePort.links.Add(localConnection);
            localConnection.BindRemote();
            return localConnection;
        }
        
        #endregion
    }
}