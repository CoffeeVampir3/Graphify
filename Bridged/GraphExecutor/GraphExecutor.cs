using System;
using Sirenix.OdinInspector;

namespace GraphFramework.GraphExecutor
{
    /// <summary>
    /// This class is responsible for executing the graph at runtime, during edit/play
    /// mode this class also creates our editor/graph link.
    /// </summary>
    [Serializable]
    public partial class GraphExecutor
    {
        public RuntimeNode currentNode = null;
        public RuntimeNode previousNode = null;
        public bool firstStep = true;

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

        public void EvaluateEditor()
        {
            if (!inEditor || !isEditorLinkedStub.Invoke()) return;
            if (previousNode != null)
                runtimeNodeExitedEditor.Invoke(previousNode);
            runtimeNodeVisitedEditor.Invoke(currentNode);
        }

        //TODO:: Odin dependency for testing only.
        [Button]
        public void Reset()
        {
            currentNode = null;
            previousNode = null;
            firstStep = true;
        }

        /// <summary>
        /// Evaluates the current node and walks the graph to whatever node is returned by
        /// the evaluated node.
        /// </summary>
        public void WalkNode()
        {
            if (!firstStep)
            {
                previousNode = currentNode;
                if (currentNode != null)
                {
                    currentNode = currentNode.OnEvaluate();
                }
            
                EvaluateEditor();
                return;
            }
            
            //This lets us evaluate the current node (usually root) without walking forward
            //for the first step.
            currentNode.OnEvaluate();
            EvaluateEditor();
            firstStep = false;
        }
    }
}