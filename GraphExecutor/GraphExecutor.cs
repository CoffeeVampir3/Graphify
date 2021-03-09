using System;
using UnityEngine;

namespace GraphFramework.GraphExecutor
{
    /// <summary>
    /// This class is responsible for executing the graph at runtime, during edit/play
    /// mode this class also creates our editor/graph link.
    /// </summary>
    [Serializable]
    public class GraphExecutor
    {
        [SerializeField]
        public SerializedGraph targetGraph;
        [SerializeField]
        public RuntimeNode currentNode = null;

        private Func<bool> isEditorLinkedStub = null;
        private Action<RuntimeNode> runtimeNodeVisitedEditor = null;
        private GraphExecutor()
        {
            #if UNITY_EDITOR
            //isEditorLinkedStub = IsEditorLinkedToGraphWindow;
            //runtimeNodeVisitedEditor = EditorLinkedRuntimeNodeVisited;
            #endif
        }

        public void WalkNode()
        {
            if (currentNode == null)
            {
                currentNode = targetGraph.rootNode;
            }
            
            if (isEditorLinkedStub != null && isEditorLinkedStub.Invoke())
            {
                runtimeNodeVisitedEditor.Invoke(currentNode);
            }
            currentNode.OnEvaluate();
            //var firstCon = currentNode.connections.FirstOrDefault();
            //currentNode = firstCon.GetRemoteNode();
        }
    }
}