using UnityEditor;
using UnityEditor.Callbacks;

namespace GraphFramework.Editor
{
    /// <summary>
    /// Handles the case when any graph controller asset is double-clicked.
    /// </summary>
    internal class GraphClickedHandler : AssetClickedHandler<GraphBlueprint>
    {
        [OnOpenAsset(1)]
        public static bool OnSerializedGraphOpened(int instanceID, int line)
        {
            if (!IsOpenedAssetTargetType(instanceID, out var controller))
                return false;
            
            var model = AssetHelper.FindNestedAssetOfType<GraphModel>(controller);
            if (model == null)
            {
                model = GraphModel.BootstrapController(controller);
            }

            EditorWindow window = EditorWindow.GetWindow(model.graphWindowType.type);
            if (window == null || !(window is GraphfyWindow graphWindow)) return false;
            
            graphWindow.LoadGraphExternal(model);
            return true;
        }
    }
}