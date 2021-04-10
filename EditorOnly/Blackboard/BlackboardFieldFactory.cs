using System;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public static class BlackboardFieldFactory
    {
        public static VisualElement Create(string fieldKey, Type t, 
            object someObject, DataBlackboard bb, Action updateViewAction, VisualElement parent)
        {
            return new BlackboardField(fieldKey, t, someObject, bb, updateViewAction, parent);
        }
    }
}