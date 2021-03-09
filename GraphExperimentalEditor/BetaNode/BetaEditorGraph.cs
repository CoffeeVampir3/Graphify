using System.Collections.Generic;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;

namespace GraphFramework.Editor
{
    [CreateAssetMenu]
    public class BetaEditorGraph : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField]
        private string coffeeGuid;
        [SerializeReference] 
        public List<NodeModel> nodeModels = new List<NodeModel>();
        [SerializeReference] 
        public List<EdgeModel> edgeModels = new List<EdgeModel>();
        [SerializeReference] 
        public List<Link> connections = new List<Link>();
        
        public string GetCoffeeGUID()
        {
            return coffeeGuid;
        }

        public void SetCoffeeGUID(string GUID)
        {
            coffeeGuid = GUID;
        }
    }
}