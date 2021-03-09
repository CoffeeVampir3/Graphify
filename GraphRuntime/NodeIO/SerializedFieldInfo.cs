using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using VisualNovelFramework.Serialization;

namespace GraphFramework
{
    //TODO:: "When" unity supports roslyn code generation this can be removed entirely.
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