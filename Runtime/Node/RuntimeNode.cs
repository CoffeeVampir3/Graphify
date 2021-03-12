using UnityEngine;

namespace GraphFramework
{
    public abstract class RuntimeNode : ScriptableObject
    {
        public virtual RuntimeNode OnEvaluate()
        {
            return null;
        }
    }
}