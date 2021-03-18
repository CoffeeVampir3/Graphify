using UnityEditor.Experimental.GraphView;
using UnityEngine.Scripting;

namespace GraphFramework.Attributes
{
    //Using preserve attribute so we don't get this stripped out in IL2CPP compilations
    /// <summary>
    /// Defines a ValuePort as an output with the given capacity.
    /// </summary>
    public class Out : PreserveAttribute
    {
        public readonly Port.Capacity capacity;
        public Out(Port.Capacity portCapacity = Port.Capacity.Single)
        {
            capacity = portCapacity;
        }
    }
}