using UnityEngine.Scripting;

namespace GraphFramework.Attributes
{
    //Using preserve attribute so we don't get this stripped out in IL2CPP compilations
    /// <summary>
    /// Defines a ValuePort as an input.
    /// </summary>
    public class In : PreserveAttribute
    {
    }
}