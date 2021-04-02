using UnityEngine;

namespace GraphFramework
{
    public class SubgraphNode : RuntimeNode
    {
        //Referenced via GUID so it's undo-safe.
        [HideInInspector]
        public string childBpGuid;
        [HideInInspector]
        public string parentBpGuid;
    }
}