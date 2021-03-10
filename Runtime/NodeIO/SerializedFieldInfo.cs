using System;
using System.Reflection;
using UnityEngine;

namespace GraphFramework
{
    [Serializable]
    public class SerializedFieldInfo
    {
        [SerializeField]
        private string fieldName;
        [SerializeField]
        private SerializableType declaringType;

        public string FieldName => fieldName;
        
        public FieldInfo FieldFromInfo => declaringType.type.GetField(fieldName, 
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public SerializedFieldInfo(FieldInfo info)
        {
            fieldName = info.Name;
            declaringType = new SerializableType(info.DeclaringType);
        }
    }
}