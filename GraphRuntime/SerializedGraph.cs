using UnityEngine;
using VisualNovelFramework.EditorExtensions;

namespace GraphFramework
{
    public class SerializedGraph : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField]
        public string GUID;
        [SerializeField]
        public RuntimeNode rootNode;

        public string GetCoffeeGUID()
        {
            return GUID;
        }

        public void SetCoffeeGUID(string newGuid)
        {
            GUID = newGuid;
        }
    }
}