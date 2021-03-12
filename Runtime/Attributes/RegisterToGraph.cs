using System;

namespace GraphFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterToGraph : Attribute
    {
        //Using a string is not ideal but since this is cross assembly we can't reference the type.
        public readonly Type registeredGraphType = null;
        public readonly string registeredPath = null;

        public RegisterToGraph(Type registerTo, string nodePath)
        {
            registeredGraphType = registerTo;
            registeredPath = nodePath;
        }

        public RegisterToGraph(Type registerTo)
        {
            registeredGraphType = registerTo;
            registeredPath = "";
        }
    }
}