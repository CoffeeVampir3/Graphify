using UnityEditor.Experimental.GraphView;

namespace GraphFramework.Attributes
{
    //Using preserve attribute so we don't get this stripped out in IL2CPP compilations
    /// <summary>
    /// Defines a ValuePort as an input with the given capacity.
    /// </summary>
    public class In : DirectionalAttribute
    {
        public In(bool showBackingValue = false, Port.Capacity portCapacity = Port.Capacity.Single)
        {
            capacity = portCapacity;
            direction = Direction.Input;
            this.showBackingValue = showBackingValue;
        }
    }
}