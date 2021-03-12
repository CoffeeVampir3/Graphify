using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
        
        /// <summary>
        ///     Searches for an asset with the provided asset GUID
        ///     This search accounts for sub-nested assets.
        /// </summary>
        public static T FindAssetWithGUID<T>(string coffeeGUID) where T : Object, HasAssetGuid
        {
            var searchStr = "t:" + typeof(T).Name;
            var charGuids = AssetDatabase.FindAssets(searchStr);
            foreach (var chGuid in charGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(chGuid);

                var assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in assetsAtPath)
                {
                    if (!(asset is T item))
                        continue;
                    if (item.AssetGuid == coffeeGUID) return item;
                }
            }
            return null;
        }
    }
}