using UnityEngine.Scripting;

namespace GraphFramework.Attributes
{
    public abstract class DirectionalAttribute : PreserveAttribute
    {
        public Capacity capacity;
        public Direction direction;
        public ConnectionRules rules;
        public bool showBackingValue = false;
    }

    public enum Capacity
    {
        Single,
        Multi
    }

    public enum Direction
    {
        Input,
        Output
    }

    public enum ConnectionRules
    {
        Exact,
        Inherited,
        None
    }
}