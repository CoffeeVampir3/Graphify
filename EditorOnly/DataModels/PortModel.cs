using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GraphFramework.Editor
{
    [Serializable]
    public class PortModel
    {
        [SerializeReference]
        protected internal Orientation orientation;
        [SerializeReference]
        protected internal Direction direction;
        [SerializeReference]
        protected internal Port.Capacity capacity;
        [SerializeReference] 
        protected internal SerializableType portValueType = null;
        [SerializeReference] 
        protected internal string portName;
        [SerializeReference] 
        protected internal SerializedFieldInfo serializedValueFieldInfo;
        //Lookup is done via GUID because undo/redo creates a different copy.
        [SerializeReference] 
        protected internal string portGUID;

        public PortModel(Orientation orientation, 
            Direction direction, 
            Port.Capacity capacity,
            Type portValueType, 
            FieldInfo fieldInfo, string portGuid)
        {
            this.orientation = orientation;
            this.direction = direction;
            this.capacity = capacity;
            this.portValueType = new SerializableType(portValueType);
            this.serializedValueFieldInfo = new SerializedFieldInfo(fieldInfo);
            this.portName = ObjectNames.NicifyVariableName(fieldInfo.Name);
            this.portGUID = portGuid;
        }
    }
}