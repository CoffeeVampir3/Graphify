using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GraphFramework.Editor
{
    /// <summary>
    /// Serializable model of our editor graph, has some extra data for debugging and
    /// utilities.
    /// </summary>
    [CreateAssetMenu]
    public class GraphModel : ScriptableObject, HasAssetGuid
    {
        [SerializeField] 
        public SerializableType graphWindowType;
        [field: SerializeField, HideInInspector]
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

        public static GraphModel CreateNew(string savePath, 
            Type editorWindowType, Type graphControllerType)
        {
            if (AssetDatabase.LoadAllAssetsAtPath(savePath).Length != 0)
            {
                Debug.LogError("Asset already exists at path: " + savePath);
                return null;
            }
            
            GraphModel graphModel = CreateInstance<GraphModel>();
            graphModel.AssetGuid = Guid.NewGuid().ToString();

            graphModel.serializedGraphController = CreateInstance(graphControllerType) as GraphController;
            if (graphModel.serializedGraphController == null)
            {
                Debug.LogError("Failed to create a new graph controller for editor window type " +
                               editorWindowType?.Name + " named " + graphControllerType?.Name);
                return null;
            }
            
            //Important note these two graphs must share the same GUID as they're linked
            //to eachother using this GUID.
            graphModel.serializedGraphController.AssetGuid = graphModel.AssetGuid;
            graphModel.graphWindowType = new SerializableType(editorWindowType);
            
            AssetDatabase.CreateAsset(graphModel.serializedGraphController, savePath);
            graphModel.name = graphModel.serializedGraphController.name + " Editor Model";
            AssetDatabase.AddObjectToAsset(graphModel, graphModel.serializedGraphController);
            AssetDatabase.SaveAssets();

            return graphModel;
        }
    }
}