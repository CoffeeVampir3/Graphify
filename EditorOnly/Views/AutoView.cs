using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    internal static class AutoView
    {
        private static VisualElement CreateDrawer<T>(SerializedProperty prop) 
            where T : BindableElement, new()
        {
            T drawer = new T();
            drawer.BindProperty(prop);
            return drawer;
        }

        private static VisualElement WrapDrawer(string rootName, VisualElement ve)
        {
            Foldout fv = new Foldout {text = rootName};
            fv.Add(ve);
            return fv;
        }
        
        private static bool FixBrokenPropertyDrawers(SerializedProperty prop, out VisualElement drawer)
        {
            string rootName = ObjectNames.NicifyVariableName(prop.propertyPath);
            if(prop.hasChildren)
                prop.NextVisible(true);

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Vector2:
                    drawer = WrapDrawer(rootName, CreateDrawer<Vector2Field>(prop));
                    return true;
                case SerializedPropertyType.Vector3:
                    drawer = WrapDrawer(rootName, CreateDrawer<Vector3Field>(prop));
                    return true;
                case SerializedPropertyType.Rect:
                    drawer = WrapDrawer(rootName, CreateDrawer<RectField>(prop));
                    return true;
                case SerializedPropertyType.Bounds:
                    drawer = WrapDrawer(rootName, CreateDrawer<BoundsField>(prop));
                    return true;
                case SerializedPropertyType.Vector2Int:
                    drawer = WrapDrawer(rootName, CreateDrawer<Vector2IntField>(prop));
                    return true;
                case SerializedPropertyType.Vector3Int:
                    drawer = WrapDrawer(rootName, CreateDrawer<Vector3IntField>(prop));
                    return true;
                case SerializedPropertyType.RectInt:
                    drawer = WrapDrawer(rootName, CreateDrawer<RectIntField>(prop));
                    return true;
                case SerializedPropertyType.BoundsInt:
                    drawer = WrapDrawer(rootName, CreateDrawer<BoundsIntField>(prop));
                    return true;
                case SerializedPropertyType.String:
                    var tDrawer = CreateDrawer<TextField>(prop);
                    tDrawer.AddToClassList("textInputBox");
                    drawer = WrapDrawer(rootName, tDrawer);
                    return true;
            }

            drawer = null;
            return false;
        }
        
        public static void Generate(SerializedObject so, VisualElement generateTo)
        {
            var it = so.GetIterator();
            if (!it.NextVisible(true))
                return;
            
            //Descends through serialized property children & allows us to edit them.
            do
            {
                var copiedProp = it.Copy();
                //Some of unity's property drawers are broken, they will create visual
                //artifacts and possibly crash unity. So we fix those.
                if (FixBrokenPropertyDrawers(copiedProp, out var drawer))
                {
                    generateTo.Add(drawer);
                    continue;
                }
                
                copiedProp = it.Copy();
                
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

                generateTo.Add(propertyField);
            }
            while (it.NextVisible(false));
        }
    }
}