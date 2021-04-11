using System;
using UnityEngine;

namespace GraphFramework
{
    [Serializable]
    public class SubgraphNode : RuntimeNode
    {
        //Referenced via GUID so it's undo-safe.
        [HideInInspector]
        public string childBpGuid;
        [HideInInspector]
        public string parentBpGuid;
        [HideInInspector]
        public SubgraphNode childNode;

        private bool isRunning = false;
        internal override RuntimeNode Evaluate(Context evContext)
        {
            BasePort.CurrentGraphIndex = evContext.virtGraph.virtualId;

            //We returned up the stack
            if (isRunning)
            {
                isRunning = false;
                return OnEvaluate(evContext);
            }
            //We are the parent 
            if (childNode != null)
            {
                evContext.Push(this);
                var childEval = childNode.Evaluate(evContext);
                isRunning = true;
                return childEval;
            }
            
            //We are the child
            if (childNode == null)
            {
                return OnEvaluate(evContext);
            }

            return null;
        }
    }
}