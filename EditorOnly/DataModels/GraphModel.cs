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
        [SerializeField]
        public Vector3 viewPosition = Vector3.zero;
        [SerializeField] 
        public Vector3 viewZoom = Vector3.one;
        [SerializeReference] 
        protected internal List<NodeModel> nodeModels = new List<NodeModel>();
        [SerializeReference]
        protected internal List<StackModel> stackModels = new List<StackModel>();
        [SerializeReference] 
        protected internal List<EdgeModel> edgeModels = new List<EdgeModel>();
        [SerializeReference] 
        protected internal List<Link> links = new List<Link>();
        [SerializeReference]
        protected internal GraphBlueprint serializedGraphBlueprint;
        [SerializeReference]
        protected internal NodeModel rootNodeModel;

        /// <summary>
        /// Bootstraps an empty GraphController with everything the editor needs to use it.
        /// </summary>
        /// <param name="savePath">The path we're saving the model to.</param>
        /// <param name="editorWindowType">The editor window type to use when this opens.</param>
        /// <param name="graphControllerType">The graph controller this model is associated to.</param>
        /// <returns>The new GraphModel</returns>
        public static GraphModel BootstrapController(GraphBlueprint graphBlueprint)
        {
            GraphModel graphModel = CreateInstance<GraphModel>();
            graphModel.name = "Editor Model";
            graphModel.serializedGraphBlueprint = graphBlueprint;

            var rootType = NodeRegistrationResolver.GetRegisteredRootNodeType(graphBlueprint.GetType());
            if (rootType == null)
            {
                //Get registered root node type will generate an error, just return here.
                return null;
            }
            
            graphModel.graphWindowType = new SerializableType(typeof(GraphfyWindow));
            
            EditorUtility.SetDirty(graphModel);
            EditorUtility.SetDirty(graphModel.serializedGraphBlueprint);
            try
            {
                AssetDatabase.StartAssetEditing();
                AssetDatabase.AddObjectToAsset(graphModel, graphBlueprint);
                graphModel.rootNodeModel = NodeModel.InstantiateModel("Root Node", graphModel, rootType);
                graphModel.serializedGraphBlueprint.rootNode = graphModel.rootNodeModel.RuntimeData;
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