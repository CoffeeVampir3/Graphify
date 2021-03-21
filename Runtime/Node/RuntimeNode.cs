using System.Runtime.CompilerServices;
using UnityEngine;

//Internals visible to graphify bridge so we can hide away potentially dangerous evaluate call.
[assembly: InternalsVisibleTo("GraphFramework.GraphifyBridge")]
namespace GraphFramework
{
    public abstract class RuntimeNode : ScriptableObject
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// Evaluates a node for the given graph id.
        /// </summary>
        internal RuntimeNode Evaluate(int graphIndex)
        {
            //Make sure we're looking up the right graph index when we lookup our port values.
            ValuePort.CurrentGraphIndex = graphIndex;
            return OnEvaluate(graphIndex);
        }

        /// <summary>
        /// Evaluates a node.
        /// </summary>
        /// <param name="contextId">The context Id of the Virtual Graph this node belongs to.</param>
        protected virtual RuntimeNode OnEvaluate(int contextId)
        {
            return null;
        }
    }
}