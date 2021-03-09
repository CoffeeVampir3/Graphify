using UnityEngine;

namespace GraphFramework
{
    public class SerializedGraph : ScriptableObject
    {
        [SerializeField]
        public string GUID;
        [SerializeField]
        public RuntimeNode rootNode;
    }
}