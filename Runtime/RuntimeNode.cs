using UnityEngine;

namespace GraphFramework
{
    public class RuntimeNode : ScriptableObject
    {
        public virtual RuntimeNode OnEvaluate()
        {
            Debug.Log(this.name);
            return null;
        }
    }
}