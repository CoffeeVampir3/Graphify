using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    /// <summary>
    /// A class that allows us to save global settings for our graph controller (by type)
    /// </summary>
    public class GraphSettings : ScriptableObject
    {
        [HideInInspector]
        public SerializableType registeredToControllerType;
        [SerializeField, HideInInspector] 
        private bool isDefault = false;
        public StyleSheet graphViewStyle;
        
        private static GraphSettings CreateGraphSettings(System.Type controllerType, List<GraphSettings> allSettings)
        {
            var defaultSettings = allSettings.FirstOrDefault(
                e => e.isDefault);
            
            if (defaultSettings == null)
            {
                Debug.LogError("Coffee Graph was unable to locate a default graph view settings file.");
                return null;
            }

            GraphSettings settings = Instantiate(defaultSettings);
            settings.registeredToControllerType = new SerializableType(controllerType);
            AssetDatabase.AddObjectToAsset(settings, 
                defaultSettings);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
            settings.name = "Settings For: " + controllerType.Name;
            settings.isDefault = false;
            
            return settings;
        }
        
        public static GraphSettings CreateOrGetSettings(GraphModel graphModel)
        {
            var allSettings = AssetHelper.FindAssetsOf<GraphSettings>();

            return allSettings.FirstOrDefault();
        }
    }
}