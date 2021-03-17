﻿using UnityEngine;

namespace GraphFramework
{
    public abstract class RuntimeNode : ScriptableObject
    {
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