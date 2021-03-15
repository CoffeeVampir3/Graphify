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
        
        public NodeView(NodeModel model)
        {
            nodeModel = model;
            
            var collapseButton = this.Q("collapse-button");

            //Keeps the model's expanded state in line with the view when a user changes
            //the value, this disregards code changing the value and I kind of like that.
            collapseButton.RegisterCallback<ClickEvent>(e =>
            {
                nodeModel.isExpanded = expanded;
            });
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
                Port p = AddPort(portModel.orientation, portModel.direction, 
                    portModel.capacity, portModel.portValueType.type);
                p.portName = portModel.portName;

                portToModel.Add(p, portModel);
                modelGuidToPort.Add(portModel.portGUID, p);
            }
        }
        
        private void CreatePortsFromModel()
        {
            CreatePortsFromModelList(nodeModel.inputPorts);
            CreatePortsFromModelList(nodeModel.outputPorts);
        }

        private Port AddPort(Orientation orientation,
            Direction direction,
            Port.Capacity capacity,
            System.Type type)
        {
            var port = InstantiatePort(orientation, direction, capacity, type);
            switch (direction)
            {
                case Direction.Input:
                    inputContainer.Add(port);
                    break;
                case Direction.Output:
                    outputContainer.Add(port);
                    break;
            }

            return port;
        }

        #endregion 

        private void CreateEditorFromNodeData()
        {
            var serializedNode = new SerializedObject(nodeModel.RuntimeData);
            
            if(ViewRegistrationResolver.TryGetCustom(nodeModel.RuntimeData.GetType(),
                   out var customEditorType) && 
               Activator.CreateInstance(customEditorType) is CustomNodeView editor) 
            {
                editor.CreateView(serializedNode);
                extensionContainer.Add(editor);
                return;
            }
            
            AutoView.Generate(serializedNode, extensionContainer);
        }

        private void Clean()
        {
            title = nodeModel.NodeTitle;
            expanded = nodeModel.isExpanded;
            
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