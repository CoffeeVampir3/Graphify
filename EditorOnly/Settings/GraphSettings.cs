using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class GraphSettings : ScriptableObject
    {
        [HideInInspector]
        public SerializableType targetGraphViewType;
        [SerializeField, HideInInspector] 
        private bool isDefault = false;
        public StyleSheet graphViewStyle;

        private static GraphSettings CreateGraphSettings(System.Type graphViewType, List<GraphSettings> allSettings)
        {
            var defaultSettings = allSettings.FirstOrDefault(
                e => e.isDefault);
            
            if (defaultSettings == null)
            {
                Debug.LogError("Coffee Graph was unable to locate a default graph view settings file.");
                return null;
            }

            GraphSettings settings = Instantiate(defaultSettings);
            settings.targetGraphViewType = new SerializableType(graphViewType);
            AssetDatabase.AddObjectToAsset(settings, 
                defaultSettings);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
            settings.name = "Settings For: " + graphViewType.Name;
            settings.isDefault = false;
            
            return settings;
        }
        
        public static GraphSettings CreateOrGetSettings(CoffeeGraphView graphView)
        {
            var allSettings = AssetHelper.FindAssetsOf<GraphSettings>();

            System.Type graphViewType = graphView.GetType();
            var graphSettings = allSettings.FirstOrDefault(
                e => e.targetGraphViewType.type == graphViewType);

            if (graphSettings == null)
            {
                graphSettings = CreateGraphSettings(graphViewType, allSettings);
            }
            
            return graphSettings;
        }
    }
}