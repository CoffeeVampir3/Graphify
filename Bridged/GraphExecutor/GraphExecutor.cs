using System;
using GraphFramework.Runtime;

namespace GraphFramework
{
    /// <summary>
    /// This class is responsible for executing the graph at runtime, during edit/play
    /// mode this class also creates our editor/graph link.
    /// </summary>
    [Serializable]
    public partial class GraphExecutor
    {
        public GraphController graphController;
        public RuntimeNode currentNode = null;
        public RuntimeNode nextNode = null;
        public RuntimeNode previousNode = null;
        //Allows you to look at specific graphs operating, rather than all of them at once.
        public bool shouldLinkEditor = false;
        [NonSerialized]
        private VirtualGraph virtualizedGraph;
        
        public void Initialize()
        {
            virtualizedGraph = graphController.CreateVirtualGraph();
            Reset();
        }
        
        public void Reset()
        {
            currentNode = graphController.rootNode;
            nextNode = null;
            previousNode = null;
            #if UNITY_EDITOR
            if(virtualizedGraph != null)
                EditorLinkedResetGraph(virtualizedGraph.virtualId);
            #endif
        }

        #region Editor Link
        
        #if UNITY_EDITOR
        //Gateway into the editor-only code, this branch is compiled out at runtime.
        public void EvaluateEditor()
        {
            if (!shouldLinkEditor || 
                !IsEditorLinkedToGraphWindow()) return;
            if (previousNode != null)
                EditorLinkedRuntimeNodeExited(previousNode);
            if(currentNode != null)
                EditorLinkedRuntimeNodeVisited(currentNode);
        }
        #endif
        
        #endregion

        /// <summary>
        /// Evaluates the current node and walks the graph to whatever node is returned by
        /// the evaluated node. Returns true if it can keep walking or false if there's no more nodes.
        /// </summary>
        public virtual bool Step()
        {
            if (nextNode != null)
            {
                currentNode = nextNode;
            }

            RuntimeNode tempPrev = currentNode;
            nextNode = currentNode.Evaluate(virtualizedGraph.virtualId);
            
            #if UNITY_EDITOR
            EvaluateEditor();
            #endif

            if (nextNode == null)
                return false;
            
            //This is our gateway into editor code, using this method we can get 100% of the
            //editor linker branch to compile out.
            //We set previous node AFTER calling evaluate editor, effectively evaluate editor
            //checks the *previous previous* node.
            previousNode = tempPrev;
            return true;
        }
    }
}