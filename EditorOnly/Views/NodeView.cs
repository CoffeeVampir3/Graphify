using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class NodeView : Node, MovableView
    {
        private readonly NodeModel nodeModel;
        private readonly Dictionary<Port, PortModel> portToModel = new Dictionary<Port, PortModel>();
        //Lookup via string because undo/redo creates a different copy.
        private readonly Dictionary<string, Port> modelGuidToPort = new Dictionary<string, Port>();
        private readonly Dictionary<string, DynamicPortModel> nameToDynamicPort =
            new Dictionary<string, DynamicPortModel>();

        public NodeView(NodeModel model)
        {
            nodeModel = model;
            
            var collapseButton = this.Q("collapse-button");
            var titleElement = this.Q<Label>();

            //Keeps the model's expanded state in line with the view when a user changes
            //the value, this disregards code changing the value and I kind of like that.
            collapseButton.RegisterCallback<ClickEvent>(e =>
            {
                nodeModel.isExpanded = expanded;
            });

            titleElement.pickingMode = PickingMode.Position;
            titleElement.RegisterCallback<PointerDownEvent>(e =>
            {
                if (e.clickCount != 2)
                    return;
                Renamer renamer = new Renamer(OnRenamed);
                renamer.Popup();
            });
        }

        public void OnRenamed(string newName)
        {
            this.nodeModel.NodeTitle = newName;
            OnDirty();
        }

        #region Ports

        /// <summary>
        /// Takes a port and spits out it's related model.
        /// </summary>
        public bool TryGetPortToModel(Port p, out PortModel model)
        {
            return portToModel.TryGetValue(p, out model);
        }
        
        /// <summary>
        /// Takes a model GUID and spits out it's related port.
        /// Lookup is via GUID because undo/redo creates a different copy.
        /// </summary>
        public bool TryGetModelToPort(string modelGUID, out Port p)
        {
            return modelGuidToPort.TryGetValue(modelGUID, out p);
        }

        private void CreatePortsFromModelList(List<PortModel> ports)
        {
            foreach (var portModel in ports)
            {
                VisualElement portElement;
                if (portModel is DynamicPortModel dynModel)
                {
                    nameToDynamicPort.Add(portModel.serializedValueFieldInfo.FieldName, dynModel);
                    portElement = dynModel.InitializeView(nodeModel, this);
                }
                else
                {
                    Port p = CreatePort(portModel);
                    p.portName = portModel.portName;
                    portElement = p;
                }

                switch (portModel.direction)
                {
                    case Direction.Input:
                        inputContainer.Add(portElement);
                        break;
                    case Direction.Output:
                        outputContainer.Add(portElement);
                        break;
                }
            }
        }
        
        private void CreatePortsFromModel()
        {
            CreatePortsFromModelList(nodeModel.portModels);
        }

        protected internal Port CreatePort(PortModel model)
        {
            var port = InstantiatePort(model.orientation, model.direction, 
                model.capacity, model.portValueType.type);
            portToModel.Add(port, model);
            modelGuidToPort.Add(model.portGUID, port);
            return port;
        }

        protected internal void RemovePort(PortModel model)
        {
            if (!TryGetModelToPort(model.portGUID, out var port))
                return;
            port.parent.Remove(port);
            portToModel.Remove(port);
            modelGuidToPort.Remove(model.portGUID);
        }

        #endregion

        protected internal void RegisterDynamicPort(string fieldName, IntegerField sizeController)
        {
            if (!nameToDynamicPort.TryGetValue(fieldName, out var dynPort))
                return;
            dynPort.SetupSizeController(sizeController);
        }

        private void CreateEditorFromNodeData()
        {
            var serializedNode = new SerializedObject(nodeModel.RuntimeData);
            
            if(ViewRegistrationResolver.TryGetCustom(nodeModel.RuntimeData.GetType(),
                   out var customEditorType) && 
               Activator.CreateInstance(customEditorType) is CustomNodeView editor) 
            {
                editor.CreateView(serializedNode, nodeModel.RuntimeData);
                extensionContainer.Add(editor);
                return;
            }
            
            //Auto view.
            this.Generate(serializedNode, nodeModel.RuntimeData, extensionContainer);
        }

        /// <summary>
        /// Updates this view to be consistent with the data model.
        /// </summary>
        private void Clean()
        {
            title = nodeModel.NodeTitle;
            expanded = nodeModel.isExpanded;
            name = nodeModel.NodeTitle;
            
            //We don't want to change our position if the node is stacked.
            if(nodeModel.stackedOn == null)
                SetPosition(nodeModel.Position);
            
            RefreshExpandedState();
            cleanImpending = false;
        }

        //Gives us a maximum refresh rate of 250MS, resulting in a minor performance boost with
        //basically no cost.
        private bool cleanImpending = false;
        private bool firstClean = true;
        private void RequestClean()
        {
            //This is so when the graph first loads we have a shortcut, otherwise
            //we'd get teleporting nodes on initialization!
            if (firstClean)
            {
                RefreshPorts();
                Clean();
                firstClean = false;
                return;
            }
            if (cleanImpending) return;
            cleanImpending = true;
            schedule.Execute(Clean).StartingIn(250);
        }

        /// <summary>
        /// Sets this view to be updated to match it's data model.
        /// </summary>
        public void OnDirty()
        {
            RequestClean();
        }

        public void Display()
        {
            CreatePortsFromModel();
            CreateEditorFromNodeData();
            
            OnDirty();
        }
    }
}