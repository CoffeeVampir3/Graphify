using UnityEditor;
using UnityEditor.Callbacks;

namespace GraphFramework.Editor
{
    internal class GraphClickedHandler : AssetClickedHandler<GraphController>
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
            if (window == null || !(window is CoffeeGraphWindow graphWindow)) return false;
            
            graphWindow.LoadGraphExternal(model);
            return true;
        }
    }
}