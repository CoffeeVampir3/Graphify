using UnityEditor;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public abstract class CustomNodeView : VisualElement
    {
        public abstract void CreateView(SerializedObject serializedRuntimeData, RuntimeNode forNode);
    }
}