using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class DynamicPortView : VisualElement
    {
        public DynamicPortView()
        {
            style.flexGrow = 1;
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);
        }
    }
}