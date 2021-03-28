using System;
using System.Collections.Generic;
using System.Reflection;
using GraphFramework.Attributes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    internal static class AutoView
    {
        #region Directional Attribute Handler
        
        private static readonly Dictionary<string, DirectionalAttribute> nameToDirAttrib
            = new Dictionary<string,DirectionalAttribute>();
        private static readonly Dictionary<string, Type> nameToType = 
            new Dictionary<string, Type>();
        
        private static bool ShouldDrawBackingField(SerializedProperty it)
        {
            return nameToDirAttrib.TryGetValue(it.propertyPath, out var attrib) && 
                   attrib.showBackingValue;
        }

        /// <summary>
        /// Sets up our dynamic list size field to have a change listener. Yes, UITK makes
        /// this very straightforward -,-".
        /// </summary>
        private static void SetupDynamics(NodeView view, SerializedProperty it, PropertyField pf)
        {
            if (!nameToType.TryGetValue(it.propertyPath, out var portType))
                return;

            if (!typeof(DynamicValuePort).IsAssignableFrom(portType))
                return;
            
            void OnDynamicGeometryChanged(GeometryChangedEvent evnt)
            {
                if (evnt.currentTarget != pf)
                    return;
                var iField = pf.Q<IntegerField>();
                //who fucking knows dude. What the fuck even is happening here.
                if (iField == null)
                    return;
                
                view.RegisterDynamicPort(pf.bindingPath, iField);
                pf.UnregisterCallback<GeometryChangedEvent>(OnDynamicGeometryChanged);
            }

            pf.RegisterCallback<GeometryChangedEvent>(OnDynamicGeometryChanged);
        }

        private static void ReadFieldData(Type nodeType)
        {
            nameToDirAttrib.Clear();
            nameToType.Clear();
            var fields = nodeType.GetFields(BindingFlags.Instance | BindingFlags.Public
                                                                  | BindingFlags.NonPublic);
            
            foreach (var field in fields)
            {
                var attribs = field.GetCustomAttributes();
                foreach (var attrib in attribs)
                {
                    if (!(attrib is DirectionalAttribute attr)) continue;
                    nameToDirAttrib.Add(field.Name, attr);
                    nameToType.Add(field.Name, field.FieldType);
                }
            }
        }
        
        #endregion
        
        public static void Generate(this NodeView view, SerializedObject so, RuntimeNode node, VisualElement generateTo)
        {
            var it = so.GetIterator();
            if (!it.NextVisible(true))
                return;
            
            nameToDirAttrib.Clear();
            ReadFieldData(node.GetType());
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

                SetupDynamics(view, it, propertyField);
                if(ShouldDrawBackingField(it))
                    generateTo.Add(propertyField);
            }
            while (it.NextVisible(false));
        }
    }
}