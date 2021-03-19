using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class NavBlackboardNodeItem : VisualElement
    {
        public readonly Label label;
        public Node targetNode;
        
        public NavBlackboardNodeItem()
        {
            label = new Label {pickingMode = PickingMode.Ignore};
            Add(label);
        }
    }
}