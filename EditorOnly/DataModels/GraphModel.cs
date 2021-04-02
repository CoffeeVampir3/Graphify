using System;
using System.Collections.Generic;
using Graphify.Runtime;
using UnityEditor;
using UnityEngine;

namespace GraphFramework.Editor
{
    /// <summary>
    /// Serializable model of our editor graph, holds all the persistent model data for a graph
    /// has some extra data for debugging and utilities.
    /// </summary>
    public class GraphModel : ScriptableObject, HasAssetGuid
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
        public GraphModel parentGraph = null;
        [SerializeReference]
        public List<GraphModel> childGraphs = null;
        [SerializeReference] 
        protected internal List<Link> links = new List<Link>();
        [SerializeReference]
        public GraphBlueprint serializedGraphBlueprint;
        [SerializeReference]
        protected internal NodeModel rootNodeModel;
        [NonSerialized] 
        public GraphifyView view = null;
        [field: SerializeField]
        public string AssetGuid { get; set; }

        public void SetParent(GraphModel parentGraphModel, GraphBlueprint blueprint)
        {
            parentGraph = parentGraphModel;
            serializedGraphBlueprint.parentGraph = blueprint;
        }

        public void AddChild(GraphModel childGraphModel, GraphBlueprint blueprint)
        {
            childGraphs.Add(childGraphModel);
            serializedGraphBlueprint.childGraphs.Add(blueprint);
        }

        public void DeleteChildGraph(GraphModel childGraph)
        {
            GraphBlueprint childBlueprint = childGraph.serializedGraphBlueprint;
            GraphBlueprint parentBlueprint = serializedGraphBlueprint;

            parentBlueprint.childGraphs.Remove(childBlueprint);
            childGraph.parentGraph.childGraphs.Remove(childGraph);
        }

        public static GraphModel GetModelFromBlueprint(GraphBlueprint blueprint)
        {
            if (blueprint == null)
                return null;
            return AssetHelper.FindNestedAssetOfType<GraphModel>(blueprint, blueprint.editorGraphGuid);
        }

        public static GraphBlueprint GetBlueprintByGuid(GraphModel parent, string guid)
        {
            if (parent == null)
                return null;
            return AssetHelper.FindNestedAssetOfType<GraphBlueprint>(parent, guid);
        }

        public GraphModel CreateChildGraph(GraphModel parentModel, NodeModel parentNode, Type blueprintType)
        {
            GraphModel graphModel = CreateInstance<GraphModel>();
            GraphBlueprint graphBlueprint = CreateInstance(blueprintType) as GraphBlueprint;
            if (graphBlueprint == null)
                return null;

            graphModel.AssetGuid = Guid.NewGuid().ToString();

            graphModel.name = AssetGuid;
            graphModel.serializedGraphBlueprint = graphBlueprint;
            graphModel.serializedGraphBlueprint.Bootstrap(graphModel.AssetGuid);
            graphBlueprint.name = graphBlueprint.AssetGuid;

            graphModel.SetParent(this, graphBlueprint);
            AddChild(graphModel, graphBlueprint);

            graphModel.graphWindowType = new SerializableType(typeof(GraphfyWindow));
            
            EditorUtility.SetDirty(graphModel);
            EditorUtility.SetDirty(graphModel.serializedGraphBlueprint);
            try
            {
                AssetDatabase.StartAssetEditing();
                AssetDatabase.AddObjectToAsset(graphModel, parentModel);
                AssetDatabase.AddObjectToAsset(graphBlueprint, parentModel);
                graphModel.rootNodeModel = parentNode;
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

            graphModel.AssetGuid = Guid.NewGuid().ToString();
            graphModel.serializedGraphBlueprint.Bootstrap(graphModel.AssetGuid);
            
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