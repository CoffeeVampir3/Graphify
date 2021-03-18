using System;

namespace GraphFramework.Attributes
{
    /// <summary>
    /// Registers this object to the provided GraphController type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterTo : Attribute
    {
        public readonly Type registeredGraphType = null;
        public readonly string registeredPath = null;
        
        /// <summary>
        /// Registers to the provided controller with the given unique searchable path.
        /// (Use no path for registering root nodes if they shouldn't be creatable.)
        /// </summary>
        public RegisterTo(Type registerTo, string nodePath = "")
        {
            registeredGraphType = registerTo;
            registeredPath = nodePath;
        }
    }
}