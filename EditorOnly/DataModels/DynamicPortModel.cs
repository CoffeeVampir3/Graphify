using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    [Serializable]
    public class DynamicPortModel : PortModel
    {
        [SerializeReference]
        public List<PortModel> dynamicPorts = new List<PortModel>();
        [NonSerialized]
        public DynamicPortView portView;
        [NonSerialized]
        public NodeView nodeView;
        [NonSerialized]
        public NodeModel nodeModel;
        [NonSerialized] 
        public IntegerField sizeField;

        public PortModel ModelByIndex(int i)
        {
            return dynamicPorts[i];
        }

        public DynamicPortView InitializeView(NodeModel model, NodeView view)
        {
            nodeView = view;
            nodeModel = model;
            portView = new DynamicPortView(portName, this);

            for (int i = 0; i < dynamicPorts.Count; i++)
            {
                Port p = nodeView.CreatePort(dynamicPorts[i]);
                p.portName = dynamicPorts[i].portName + "-" + i;
                portView.Add(p);
            }
            return portView;
        }

        public void SetupSizeController(IntegerField sizeController)
        {
            sizeField = sizeController;
            sizeField.RegisterValueChangedCallback(Resize);
            var evt = ChangeEvent<int>.GetPooled(0, sizeField.value);
            evt.target = sizeField;
            sizeField.SendEvent(evt);
        }

        private BasePort GetBasePort()
        {
            var remotePortInfo = serializedValueFieldInfo.FieldFromInfo;
            if (remotePortInfo == null)
                return null;

            BasePort port = remotePortInfo.GetValue(nodeModel.RuntimeData) as BasePort;
            return port;
        }

        private void DeleteLinksFromPort(PortModel port)
        {
            BasePort basePort = GetBasePort();
            if (basePort == null)
            {
                Debug.LogError(
                    "Attempted to clean links from: " + portName + " but it had a corrupted link record!");
                return;
            }

            for (int i = basePort.links.Count - 1; i >= 0; i--)
            {
                if (port.linkGuids.Contains(basePort.links[i].GUID))
                {
                    basePort.links.RemoveAt(i);
                    var parentView = nodeView.GetFirstAncestorOfType<GraphifyView>();
                    parentView.DeletePortEdges(nodeModel, port);
                }
            }
        }

        public void OnAddClicked()
        {
            var evt = ChangeEvent<int>.GetPooled(sizeField.value, sizeField.value+1);
            sizeField.SetValueWithoutNotify(sizeField.value+1);
            evt.target = sizeField;
            sizeField.SendEvent(evt);
        }

        public void OnRemoveClicked()
        {
            if (sizeField.value <= 0)
                return;
            var evt = ChangeEvent<int>.GetPooled(sizeField.value, sizeField.value-1);
            sizeField.SetValueWithoutNotify(sizeField.value-1);
            evt.target = sizeField;
            sizeField.SendEvent(evt);
        }

        public void Resize(ChangeEvent<int> change)
        {
            var newSize = change.newValue;
            if (newSize == dynamicPorts.Count)
                return;

            if (newSize > dynamicPorts.Count)
            {
                for (int i = dynamicPorts.Count; i < newSize; i++)
                {
                    PortModel model = new PortModel(orientation, direction,
                        capacity, portValueType.type, serializedValueFieldInfo.FieldFromInfo, 
                        Guid.NewGuid().ToString(), i);
                    dynamicPorts.Add(model);
                    Port p = nodeView.CreatePort(model);
                    p.portName = dynamicPorts[i].portName + "-" + i;
                    portView.Add(p);
                }

                return;
            }
            for (int i = dynamicPorts.Count - 1; i >= newSize; i--)
            {
                PortModel model = dynamicPorts[i];
                DeleteLinksFromPort(model);
                nodeView.RemovePort(model);
                dynamicPorts.RemoveAt(i);
            }
        }
        
        public DynamicPortModel(Orientation orientation, Direction direction, 
            Port.Capacity capacity, Type portValueType, FieldInfo fieldInfo, 
            string portGuid) : 
            base(orientation, direction, capacity, portValueType, fieldInfo, portGuid)
        {
        }
    }
}