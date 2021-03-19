using UnityEngine.Scripting;

namespace GraphFramework.Attributes
{
    public abstract class DirectionalAttribute : PreserveAttribute
    {
        public Capacity capacity;
        public Direction direction;
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
}