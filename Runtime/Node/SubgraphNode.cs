using System;
using UnityEngine;

namespace GraphFramework
{
    [Serializable]
    public class SubgraphNode : RuntimeNode
    {
        //Referenced via GUID so it's undo-safe.
        public string childBpGuid;
        public string parentBpGuid;
        public RuntimeNode childNode;
        
        [NonSerialized]
        private RuntimeNode currentNode = null;
        [NonSerialized]
        private RuntimeNode nextNode = null;

        internal override RuntimeNode Evaluate(int graphIndex)
        {
            //Make sure we're looking up the right graph index when we lookup our port values.
            BasePort.CurrentGraphIndex = graphIndex;
            if (childNode != null)
            {
                var evaluation = childNode.Evaluate(graphIndex);
                if (evaluation == null)
                {
                    return OnEvaluate(graphIndex); 
                }

                return this;
            }
            
            Debug.Log("Stage 1 ");
            if (currentNode == null && nextNode == null)
            {
                var evaluation = OnEvaluate(graphIndex);
                if (evaluation == null) return null;
                
                currentNode = this;
                nextNode = evaluation;
                return nextNode;
            }
            
            Debug.Log("Stage 2 ");
            if (nextNode != null)
            {
                currentNode = nextNode;
                nextNode = null;
            }
            nextNode = currentNode.Evaluate(graphIndex);

            Debug.Log("Stage 3");
            if (nextNode == null)
            {
                Debug.Log("Next node is null.");
                currentNode = null;
                nextNode = null;
                return null;
            }

            Debug.Log("Stage 4");
            Debug.Log(nextNode.name);
            return nextNode;
        }
    }
}