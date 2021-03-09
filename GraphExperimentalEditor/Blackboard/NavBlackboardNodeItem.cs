using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class NavBlackboardNodeItem : VisualElement
    {
        public readonly Foldout foldout;
        public Node targetNode;
        
        public NavBlackboardNodeItem()
        {
            foldout = new Foldout();
            foldout.SetValueWithoutNotify(false);

            Add(foldout);
        }
    }
}