using System;

namespace GraphFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterNode : GraphRegisterable
    {
        public RegisterNode(Type registerTo, string nodePath)
        {
            registeredGraphType = registerTo;
            registeredPath = nodePath;
        }

        public RegisterNode(Type registerTo)
        {
            registeredGraphType = registerTo;
            registeredPath = "";
        }
    }
}