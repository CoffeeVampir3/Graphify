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
        internal List<PortModel> dynamicPorts = new List<PortModel>();
        [SerializeField] 
        internal int minSize = 0;
        [SerializeField] 
        internal int maxSize = int.MaxValue;
        [NonSerialized] 
        internal GraphifyView parentGraph;
        [NonSerialized]
        internal DynamicPortView portView;
        [NonSerialized]
        internal NodeView nodeView;
        [NonSerialized]
        internal NodeModel nodeModel;
        [NonSerialized] 
        internal IntegerField sizeField;

        public PortModel ModelByIndex(int i)
        {
            return dynamicPorts[i];
        }

        public DynamicPortView InitializeView(NodeModel model, NodeView nv)
        {
            nodeView = nv;
            nodeModel = model;
            portView = new DynamicPortView(portName, this, direction);

            portView.RegisterCallback<AttachToPanelEvent>(e =>
            {
                parentGraph = portView.GetFirstAncestorOfType<GraphifyView>();
            });

            for (int i = 0; i < dynamicPorts.Count; i++)
            {
                Port p = nodeView.CreatePort(dynamicPorts[i]);
                p.portName = dynamicPorts[i].portName + "-" + i;
                portView.Add(p);
            }
            
            if(dynamicPorts.Count == 0)
                Resize(0);
            
            return portView;
        }

        public void SetupSizeController(IntegerField sizeController)
        {
            sizeField = sizeController;
            sizeField.RegisterValueChangedCallback(ResizeSurrogate);
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

        internal void DeleteAllLinks()
        {
            for (int i = dynamicPorts.Count - 1; i >= 0; i--)
            {
                DeleteLinksFromPort(dynamicPorts[i]);
            }
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
            
            for (int i = basePort.links.ToArray().Length - 1; i >= 0; i--)
            {
                if (port.linkGuids.Contains(basePort.links[i].GUID))
                {
                    basePort.links.RemoveAt(i);
                    parentGraph.DeletePortEdges(port);
                }
            }
        }

        public void OnAddClicked()
        {
            if (sizeField != null)
            {
                var evt = ChangeEvent<int>.GetPooled(sizeField.value, sizeField.value+1);
                sizeField.SetValueWithoutNotify(sizeField.value+1);
                evt.target = sizeField;
                sizeField.SendEvent(evt);
                return;
            }
            
            Resize(dynamicPorts.Count+1);
        }

        public void OnRemoveClicked()
        {
            if (sizeField != null && sizeField.value <= 0)
            {
                var evt = ChangeEvent<int>.GetPooled(sizeField.value, sizeField.value-1);
                sizeField.SetValueWithoutNotify(sizeField.value-1);
                evt.target = sizeField;
                sizeField.SendEvent(evt);
                return;
            }

            if (dynamicPorts.Count > 0)
            {
                Resize(dynamicPorts.Count-1);
            }
        }

        public void ResizeSurrogate(ChangeEvent<int> change)
        {
            Resize(change.newValue);
        }

        public void Resize(int newSize)
        {
            if (newSize == dynamicPorts.Count && newSize != 0)
                return;
            
            if (newSize > maxSize)
            {
                newSize = maxSize;
                sizeField?.SetValueWithoutNotify(newSize);
            }
            if (newSize < minSize)
            {
                newSize = minSize;
                sizeField?.SetValueWithoutNotify(newSize);
            }

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