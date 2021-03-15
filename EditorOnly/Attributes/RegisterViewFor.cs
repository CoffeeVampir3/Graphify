using System;
using UnityEngine;

namespace GraphFramework.EditorOnly.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterViewFor : Attribute
    {
        public readonly Type runtimeNodeType;
        /// <summary>
        /// Registers a custom view for the given runtime node type.
        /// </summary>
        public RegisterViewFor(Type runtimeNodeType)
        {
            if (!typeof(RuntimeNode).IsAssignableFrom(runtimeNodeType))
            {
                Debug.LogError("Attempted to register a custom view for something other than a runtime node.");
                return;
            }
            
            this.runtimeNodeType = runtimeNodeType;
        }
    }
}