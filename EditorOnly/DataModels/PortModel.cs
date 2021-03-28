using System;
using System.Collections.Generic;
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
        protected internal SerializableType portValueType = null;
        [SerializeReference] 
        protected internal SerializedFieldInfo serializedValueFieldInfo;
        [SerializeField] 
        protected internal List<string> linkGuids = new List<string>();
        [SerializeField] 
        protected internal string portName;
        //Lookup is done via GUID because undo/redo creates a different copy.
        [SerializeField] 
        protected internal string portGUID;
        [SerializeField] 
        protected internal int dynamicIndex;
        [SerializeField]
        protected internal Orientation orientation;
        [SerializeField]
        protected internal Direction direction;
        [SerializeField]
        protected internal Port.Capacity capacity;

        public PortModel(Orientation orientation, 
            Direction direction, 
            Port.Capacity capacity,
            Type portValueType, 
            FieldInfo fieldInfo, 
            string portGuid,
            int dynamicIndex = -1)
        {
            this.orientation = orientation;
            this.direction = direction;
            this.capacity = capacity;
            this.portValueType = new SerializableType(portValueType);
            this.serializedValueFieldInfo = new SerializedFieldInfo(fieldInfo);
            this.portName = ObjectNames.NicifyVariableName(fieldInfo.Name);
            this.portGUID = portGuid;
            this.dynamicIndex = dynamicIndex;
        }
    }
}