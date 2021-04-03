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

        internal override RuntimeNode Evaluate(Context evContext)
        {
            BasePort.CurrentGraphIndex = evContext.graphId;
            
            //Parent 
            if (childNode == null) return null;
            
            var childEval = 
                childNode.Evaluate(new Context(evContext, this, evContext.graphId));
            if (childEval != null)
            {
                return childEval;
            }

            return OnEvaluate(evContext.graphId);

        }
    }
}