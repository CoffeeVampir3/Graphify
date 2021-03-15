using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GraphFramework.Editor
{
    /// <summary>
    /// Serializable model of our editor graph, holds all the persistent model data for a graph
    /// has some extra data for debugging and utilities.
    /// </summary>
    public class GraphModel : ScriptableObject
    {
        [SerializeField] 
        public SerializableType graphWindowType;

        [SerializeReference] 
        protected internal List<NodeModel> nodeModels = new List<NodeModel>();
        [SerializeReference]
        protected internal List<StackModel> stackModels = new List<StackModel>();
        [SerializeReference] 
        protected internal List<EdgeModel> edgeModels = new List<EdgeModel>();
        [SerializeReference] 
        protected internal List<Link> links = new List<Link>();
        [SerializeReference]
        protected internal GraphController serializedGraphController;
        [SerializeReference] 
        protected internal NodeModel rootNodeModel;

        /// <summary>
        /// Bootstraps an empty GraphController with everything the editor needs to use it.
        /// </summary>
        /// <param name="savePath">The path we're saving the model to.</param>
        /// <param name="editorWindowType">The editor window type to use when this opens.</param>
        /// <param name="graphControllerType">The graph controller this model is associated to.</param>
        /// <returns>The new GraphModel</returns>
        public static GraphModel BootstrapController(GraphController graphController)
        {
            GraphModel graphModel = CreateInstance<GraphModel>();
            graphModel.name = "Editor Model";
            graphModel.serializedGraphController = graphController;

            var rootType = NodeRegistrationResolver.GetRegisteredRootNodeType(graphController.GetType());
            if (rootType == null)
            {
                //Get registered root node type will generate an error, just return here.
                return null;
            }
            
            //TODO:: Add support for custom window.
            graphModel.graphWindowType = new SerializableType(typeof(CoffeeGraphWindow));
            
            EditorUtility.SetDirty(graphModel);
            EditorUtility.SetDirty(graphModel.serializedGraphController);
            try
            {
                AssetDatabase.StartAssetEditing();
                AssetDatabase.AddObjectToAsset(graphModel, graphController);
                graphModel.rootNodeModel = NodeModel.InstantiateModel("Root Node", graphModel, rootType);
                graphModel.serializedGraphController.rootNode = graphModel.rootNodeModel.RuntimeData;
                EditorUtility.SetDirty(graphModel.rootNodeModel.RuntimeData);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            return graphModel;
        }
    }
}