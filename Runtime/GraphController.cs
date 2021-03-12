using GraphFramework.Editor;
using UnityEngine;

namespace GraphFramework
{
    public abstract class GraphController : ScriptableObject, HasAssetGuid
    {
        [SerializeField]
        public RuntimeNode rootNode;
        [field: SerializeField]
        public string AssetGuid { get; set; }
    }
}