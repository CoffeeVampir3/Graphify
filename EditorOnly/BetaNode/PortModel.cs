using System;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GraphFramework.Editor
{
    [Serializable]
    public class PortModel
    {
        [SerializeReference]
        public Orientation orientation;
        [SerializeReference]
        public Direction direction;
        [SerializeReference]
        public Port.Capacity capacity;
        [SerializeReference] 
        public SerializableType portValueType = null;
        [SerializeReference] 
        public SerializedFieldInfo serializedValueFieldInfo;
        //Lookup is done via GUID because undo/redo creates a different copy.
        [SerializeReference] 
        public string portGUID;

        public PortModel(Orientation orientation, 
            Direction direction, 
            Port.Capacity capacity, 
            Type portType, 
            FieldInfo fieldInfo)
        {
            this.orientation = orientation;
            this.direction = direction;
            this.capacity = capacity;
            this.portValueType = new SerializableType(portType);
            this.serializedValueFieldInfo = new SerializedFieldInfo(fieldInfo);
            portGUID = Guid.NewGuid().ToString();
        }
    }
}