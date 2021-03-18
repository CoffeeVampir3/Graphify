using UnityEditor.Experimental.GraphView;
using UnityEngine.Scripting;

namespace GraphFramework.Attributes
{
    public abstract class DirectionalAttribute : PreserveAttribute
    {
        public Port.Capacity capacity;
        public Direction direction;
        public bool showBackingValue = false;
    }
}