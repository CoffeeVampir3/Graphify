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
        /// </summary>
        public RegisterTo(Type registerTo, string nodePath)
        {
            registeredGraphType = registerTo;
            registeredPath = nodePath;
        }

        /// <summary>
        /// Registers to the provided controller no searchable path, used primarily for RootNode.
        /// </summary>
        public RegisterTo(Type registerTo)
        {
            registeredGraphType = registerTo;
            registeredPath = "";
        }
    }
}