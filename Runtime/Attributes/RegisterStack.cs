using System;

namespace GraphFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterStack : GraphRegisterable
    {
        public readonly Type[] validNodeTypes = null;

        /// <summary>
        /// Registers a stack node, by default anything can be stacked on them.
        /// </summary>
        public RegisterStack(Type registerTo, string nodePath) // params Type[] validNodes
        {
            //validNodeTypes = validNodes;
            registeredGraphType = registerTo;
            registeredPath = nodePath;
        }
    }
}