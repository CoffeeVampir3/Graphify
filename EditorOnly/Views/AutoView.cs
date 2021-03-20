using System;
using System.Collections.Generic;
using System.Reflection;
using GraphFramework.Attributes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    internal static class AutoView
    {
        #region Directional Attribute Handler
        
        private static readonly Dictionary<string, DirectionalAttribute> nameToDirAttrib
            = new Dictionary<string,DirectionalAttribute>();
        
        private static bool ShouldDrawBackingField(SerializedProperty it)
        {
            return nameToDirAttrib.TryGetValue(it.propertyPath, out var attrib) && 
                   attrib.showBackingValue;
        }

        private static void ReadDirectionalAttribs(Type nodeType)
        {
            nameToDirAttrib.Clear();
            var fields = nodeType.GetFields(BindingFlags.Instance | BindingFlags.Public
                                                                  | BindingFlags.NonPublic);
            
            foreach (var field in fields)
            {
                var attribs = field.GetCustomAttributes();
                foreach (var attrib in attribs)
                {
                    if (!(attrib is DirectionalAttribute attr)) continue;
                    nameToDirAttrib.Add(field.Name, attr);
                }
            }
        }
        
        #endregion
        
        public static void Generate(SerializedObject so, RuntimeNode node ,VisualElement generateTo)
        {
            var it = so.GetIterator();
            if (!it.NextVisible(true))
                return;
            
            nameToDirAttrib.Clear();
            ReadDirectionalAttribs(node.GetType());
            //Descends through serialized property children & allows us to edit them.
            do
            {
                var copiedProp = it.Copy();

                //Otherwise lay out like normal.
                var propertyField = new PropertyField(copiedProp) 
                    { name = it.propertyPath };

                //Bind the property so we can edit the values.
                propertyField.Bind(so);

                //This ignores the label name field, it's ugly.
                if (it.propertyPath == "m_Script" && 
                    so.targetObject != null) 
                {
                    propertyField.SetEnabled(false);
                    propertyField.visible = false;
                    continue;
                }

                if(ShouldDrawBackingField(it))
                    generateTo.Add(propertyField);
            }
            while (it.NextVisible(false));
        }
    }
}