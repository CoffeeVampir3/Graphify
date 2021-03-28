using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GraphFramework.Editor
{
    [Serializable]
    public class DynamicPortModel : PortModel
    {
        [SerializeReference]
        public List<PortModel> dynamicPorts = new List<PortModel>();
        [NonSerialized]
        public DynamicPortView portView;
        public NodeView nodeView;
        public NodeModel nodeModel;

        public PortModel ModelByIndex(int i)
        {
            return dynamicPorts[i];
        }

        public DynamicPortView InitializeView(NodeModel model, NodeView view)
        {
            nodeView = view;
            nodeModel = model;
            portView = new DynamicPortView();

            for (int i = 0; i < dynamicPorts.Count; i++)
            {
                Port p = nodeView.CreatePort(dynamicPorts[i]);
                p.portName = dynamicPorts[i].portName + "-" + i;
                portView.Add(p);
            }
            return portView;
        }

        private BasePort GetBasePort()
        {
            var remotePortInfo = serializedValueFieldInfo.FieldFromInfo;
            if (remotePortInfo == null)
                return null;

            BasePort port = remotePortInfo.GetValue(nodeModel.RuntimeData) as BasePort;
            return port;
        }

        private void DeleteLinksFromPort(IReadOnlyCollection<string> guidsToDelete)
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
                if (guidsToDelete.Contains(basePort.links[i].GUID))
                {
                    basePort.links.RemoveAt(i);
                }
            }
        }

        public void Resize(int newSize)
        {
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
                DeleteLinksFromPort(model.linkGuids);
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