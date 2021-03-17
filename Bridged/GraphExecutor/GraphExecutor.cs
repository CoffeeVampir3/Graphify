﻿using System;
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
        public GraphController graphController;
        
        public RuntimeNode currentNode = null;
        public RuntimeNode nextNode = null;
        public RuntimeNode previousNode = null;
        //Allows you to look at specific graphs operating, rather than all of them at once.
        public bool shouldLinkEditor = false;

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

        [Button]
        public void Reset(int graphId)
        {
            currentNode = graphController.rootNode;
            nextNode = null;
            previousNode = null;
        }

        /// <summary>
        /// Evaluates the current node and walks the graph to whatever node is returned by
        /// the evaluated node.
        /// </summary>
        public void WalkNode(int graphId)
        {
            if (nextNode != null)
                currentNode = nextNode;
            RuntimeNode tempPrev = currentNode;
            nextNode = currentNode.Evaluate(graphId);
            
            //This is our gateway into editor code, using this method we can get 100% of the
            //editor linker branch to compile out.
            #if UNITY_EDITOR
            EvaluateEditor();
            #endif
            //We set previous node AFTER calling evaluate editor, effectively evaluate editor
            //checks the *previous previous* node.
            previousNode = tempPrev;
        }
    }
}