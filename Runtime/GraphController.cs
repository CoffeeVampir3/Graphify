using GraphFramework.Runtime;
using UnityEngine;

namespace GraphFramework
{
    public abstract class GraphController : ScriptableObject
    {
        [SerializeField]
        public RuntimeNode rootNode;
        [SerializeReference]
        public GravestoneList<RuntimeNode> runtimeNodes = new GravestoneList<RuntimeNode>();
    }
}