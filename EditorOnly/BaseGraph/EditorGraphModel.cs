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
    public class EditorGraphModel : ScriptableObject, HasAssetGuid
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

        public static EditorGraphModel CreateNew(string savePath, 
            Type editorWindowType, Type graphControllerType)
        {
            if (AssetDatabase.LoadAllAssetsAtPath(savePath).Length != 0)
            {
                Debug.LogError("Asset already exists at path: " + savePath);
                return null;
            }
            
            EditorGraphModel editorGraphModel = CreateInstance<EditorGraphModel>();
            editorGraphModel.AssetGuid = Guid.NewGuid().ToString();

            editorGraphModel.serializedGraphController = CreateInstance(graphControllerType) as GraphController;
            if (editorGraphModel.serializedGraphController == null)
            {
                Debug.LogError("Failed to create a new graph controller for editor window type " +
                               editorWindowType?.Name + " named " + graphControllerType?.Name);
                return null;
            }
            
            //Important note these two graphs must share the same GUID as they're linked
            //to eachother using this GUID.
            editorGraphModel.serializedGraphController.AssetGuid = editorGraphModel.AssetGuid;
            editorGraphModel.graphWindowType = new SerializableType(editorWindowType);
            
            AssetDatabase.CreateAsset(editorGraphModel.serializedGraphController, savePath);
            editorGraphModel.name = editorGraphModel.serializedGraphController.name + " Editor Model";
            AssetDatabase.AddObjectToAsset(editorGraphModel, editorGraphModel.serializedGraphController);
            AssetDatabase.SaveAssets();

            return editorGraphModel;
        }
    }
}