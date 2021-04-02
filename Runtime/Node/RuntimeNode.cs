﻿using System.Runtime.CompilerServices;
using UnityEngine;

//Internals visible so we can hide away potentially dangerous stuff
[assembly: InternalsVisibleTo("GraphFramework.GraphifyBridge")]
[assembly: InternalsVisibleTo("GraphFramework.GraphifyEditor")]
namespace GraphFramework
{
    public abstract class RuntimeNode : ScriptableObject
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// Evaluates a node for the given graph id.
        /// </summary>
        internal virtual RuntimeNode Evaluate(int graphIndex)
        {
            //Make sure we're looking up the right graph index when we lookup our port values.
            BasePort.CurrentGraphIndex = graphIndex;
            return OnEvaluate(graphIndex);
        }

        /// <summary>
        /// Evaluates a node. Because of how virtualization works, if you need to store context-sensitive data
        /// in the node, use the provided contextId to store the values in a dictionary or lookup scheme.
        /// </summary>
        protected virtual RuntimeNode OnEvaluate(int contextId)
        {
            return null;
        }
    }
}