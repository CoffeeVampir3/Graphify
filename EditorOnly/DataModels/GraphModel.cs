using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GraphFramework.Editor
{
    /// <summary>
    /// Serializable model of our editor graph, holds all the persistent model data for a graph
    /// has some extra data for debugging and utilities.
    /// </summary>
    [CreateAssetMenu]
    public class GraphModel : ScriptableObject, HasAssetGuid
    {
        [SerializeField] 
        public SerializableType graphWindowType;
        [field: SerializeField]
        public string AssetGuid { set; get; }
        
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
            graphModel.AssetGuid = Guid.NewGuid().ToString();
            graphModel.name = "Editor Model";
            graphModel.serializedGraphController = graphController;

            var rootType = NodeRegistrationResolver.GetRegisteredRootNodeType(graphController.GetType());
            if (rootType == null)
            {
                //Get registered root node type will generate an error, just return here.
                return null;
            }

            //Important note these two graphs must share the same GUID as they're linked
            //to eachother using this GUID.
            graphModel.serializedGraphController.AssetGuid = graphModel.AssetGuid;
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