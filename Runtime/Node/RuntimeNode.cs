using UnityEngine;

namespace GraphFramework
{
    public abstract class RuntimeNode : ScriptableObject
    {
        /// <summary>
        /// Evaluates a node for the given graph id.
        /// </summary>
        public RuntimeNode Evaluate(int graphIndex)
        {
            //Make sure we're looking up the right graph index when we lookup our port values.
            ValuePort.CurrentGraphIndex = graphIndex;
            return OnEvaluate();
        }

        protected virtual RuntimeNode OnEvaluate()
        {
            return null;
        }
    }
}