using System;
using GraphFramework.Runtime;

namespace GraphFramework
{
    /// <summary>
    /// This class is responsible for executing the graph at runtime, during edit/play
    /// mode this class also creates our editor/graph link.
    /// </summary>
    [Serializable]
    public partial class GraphEvaluator
    {
        public GraphBlueprint graphBlueprint;
        private RuntimeNode currentNode = null;
        private RuntimeNode nextNode = null;
        private RuntimeNode previousNode = null;
        private Context rootContext;

        public RuntimeNode Current => currentNode;
        public RuntimeNode Next => nextNode;
        public RuntimeNode Prev => previousNode;
        
        //Allows you to look at specific graphs operating, rather than all of them at once.
        public bool shouldLinkEditor = false;
        [NonSerialized]
        private VirtualGraph virtualizedGraph;
        
        public void Initialize(Action blackboardInitialization = null)
        {
            virtualizedGraph = graphBlueprint.CreateVirtualGraph();
            rootContext = new Context(virtualizedGraph);
            Reset();
            Blackboards.virtGraph = virtualizedGraph;
            blackboardInitialization?.Invoke();
        }
        
        public void Reset()
        {
            currentNode = graphBlueprint.rootNode;
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
        /// the evaluated node. Returns the next node that will be evaluated or null if none.
        /// </summary>
        public RuntimeNode Step()
        {
            if (nextNode != null)
            {
                currentNode = nextNode;
            }

            RuntimeNode tempPrev = currentNode;
            nextNode = currentNode.Evaluate(rootContext);
            
            //This is our gateway into editor code, using this method we can get 100% of the
            //editor linker branch to compile out.
            //We set previous node AFTER calling evaluate editor, effectively evaluate editor
            //checks the *previous previous* node.
            #if UNITY_EDITOR
            EvaluateEditor();
            #endif

            if (nextNode == null && rootContext.Count() > 0)
            {
                nextNode = rootContext.Pop();
            }

            previousNode = tempPrev;
            return nextNode;
        }
    }
}