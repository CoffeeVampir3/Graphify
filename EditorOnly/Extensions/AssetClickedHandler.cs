using UnityEngine;

namespace GraphFramework.Editor
{
    internal abstract class AssetClickedHandler<Item>
        where Item : ScriptableObject
    {
        /// <summary>
        /// Helper function to check if a clicked asset is one of the type we care about, if
        /// it is, it will out the clicked asset item and return true, otherwise returns false.
        /// </summary>
        protected static bool IsOpenedAssetTargetType(int instanceID, out Item openedItem)
        {
            var targetItems = AssetHelper.FindAssetsOf<Item>();

            openedItem = null;
            foreach (var targetItem in targetItems)
            {
                if (targetItem.GetInstanceID() != instanceID) continue;
                openedItem = targetItem;
                return true;
            }

            return false;
        }
    }
}