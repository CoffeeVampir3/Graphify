using System.Collections.Generic;
using UnityEditor;

namespace GraphFramework.Editor
{
    public static class AssetExtensions
    {
        public static List<T> FindAssetsOfType<T>() where T : UnityEngine.Object
        {
            var searchStr = "t:" + typeof(T).Name;
            var charGuids = AssetDatabase.FindAssets(searchStr);
            var items = new List<T>();
            foreach (var chGuid in charGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(chGuid);
                var assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in assetsAtPath)
                {
                    if (!(asset is T item))
                        continue;
                    items.Add(item);
                }
            }

            return items;
        }
    }
}