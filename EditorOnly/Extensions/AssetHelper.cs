using System.Collections.Generic;
using Graphify.Runtime;
using UnityEditor;
using UnityEngine;

namespace GraphFramework.Editor
{
    public static class AssetHelper
    {
        /// <summary>
        /// Finds all assets of type T, accounts for subnesting.
        /// </summary>
        public static List<T> FindAssetsOf<T>() where T : UnityEngine.Object
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

        public static T FindNestedAssetOfType<T>(Object mainAsset, string searchGuid)
        where T : HasAssetGuid
        {
            var path = AssetDatabase.GetAssetPath(mainAsset);

            var objs = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var obj in objs)
            {
                if (!(obj is T item)) continue;
                if(item.AssetGuid == searchGuid)
                    return item;
            }

            return default;
        }
    }
}