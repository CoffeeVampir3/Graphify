using System;
using UnityEngine;

namespace GraphFramework.GraphExecutor
{
    /// <summary>
    /// This class is responsible for executing the graph at runtime, during edit/play
    /// mode this class also creates our editor/graph link.
    /// </summary>
    [Serializable]
    public partial class GraphExecutor
    {
        [SerializeField]
        public RuntimeNode currentNode = null;

        private bool inEditor = false;
        private Func<bool> isEditorLinkedStub = null;
        private Action<RuntimeNode> runtimeNodeVisitedEditor = null;
        private Action<RuntimeNode> runtimeNodeExitedEditor = null;
        
        //When this object is created, if we're in the unity editor we create our linker
        //functions, otherwise this amounts to a single bool overhead.
        protected GraphExecutor()
        {
            #if UNITY_EDITOR
            inEditor = true;
            isEditorLinkedStub = IsEditorLinkedToGraphWindow;
            runtimeNodeVisitedEditor = EditorLinkedRuntimeNodeVisited;
            runtimeNodeExitedEditor = EditorLinkedRuntimeNodeExited;
            #endif
        }

        [SerializeField]
        private RuntimeNode previousNode = null;
        public void WalkNode()
        {
            if (inEditor && isEditorLinkedStub.Invoke())
            {
                if(previousNode != null)
                    runtimeNodeExitedEditor.Invoke(previousNode);
                runtimeNodeVisitedEditor.Invoke(currentNode);
            }
            
            previousNode = currentNode;
            if (currentNode != null)
            {
                currentNode = currentNode.OnEvaluate();
            }
        }
    }
}