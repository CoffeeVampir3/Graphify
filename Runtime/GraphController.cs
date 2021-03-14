using UnityEngine;

namespace GraphFramework
{
    public abstract class GraphController : ScriptableObject
    {
        [SerializeField]
        public RuntimeNode rootNode;
    }
}