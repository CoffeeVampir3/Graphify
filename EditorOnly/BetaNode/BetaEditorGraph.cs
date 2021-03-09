using System.Collections.Generic;
using UnityEngine;

namespace GraphFramework.Editor
{
    [CreateAssetMenu]
    public class BetaEditorGraph : ScriptableObject
    {
        [SerializeField]
        private string coffeeGuid;
        [SerializeReference] 
        public List<NodeModel> nodeModels = new List<NodeModel>();
        [SerializeReference]
        public List<StackModel> stackModels = new List<StackModel>();
        [SerializeReference] 
        public List<EdgeModel> edgeModels = new List<EdgeModel>();
        [SerializeReference] 
        public List<Link> connections = new List<Link>();
    }
}