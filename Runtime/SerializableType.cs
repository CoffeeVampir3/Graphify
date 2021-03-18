using System;
using UnityEngine;

namespace GraphFramework
{
    [Serializable]
    public class SerializableType : ISerializationCallbackReceiver
    {
        [SerializeField, HideInInspector]
        private string serializedTypeName = "";
        [NonSerialized]
        public Type type = null;

        public SerializableType(System.Type t)
        {
            type = t;
        }

        public void OnBeforeSerialize()
        {
            if (type != null)
            {
                serializedTypeName = type.AssemblyQualifiedName;
            }
        }

        public void OnAfterDeserialize()
        {
            type = Type.GetType(serializedTypeName);
        }
    }
}