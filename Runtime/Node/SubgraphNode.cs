using System;

namespace GraphFramework
{
    [Serializable]
    public class SubgraphNode : RuntimeNode
    {
        //Referenced via GUID so it's undo-safe.
        public string childBpGuid;
        public string parentBpGuid;
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