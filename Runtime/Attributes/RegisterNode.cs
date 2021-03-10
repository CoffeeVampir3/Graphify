using System;

namespace GraphFramework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterNodeToView : Attribute
    {
        //Using a string is not ideal but since this is cross assembly we can't reference the type.
        public readonly string registeredGraphViewTypeName = null;
        public readonly string registeredPath = null;
        /// <summary>
        /// The type name (as a string) of the graph you want to register to, and the path
        /// you'd like the node to be listed as. Ex: "Math Functions/Add Node"
        /// </summary>
        public RegisterNodeToView(string registerToGraphViewTypeName, string nodePath)
        {
            registeredGraphViewTypeName = registerToGraphViewTypeName;
            registeredPath = nodePath;
        }
        /// <summary>
        /// The type name (as a string) of the graph you want to register to, this
        /// registers the node under no path, used for root nodes and specialty nodes.
        /// </summary>
        public RegisterNodeToView(string registerToGraphViewTypeName)
        {
            registeredGraphViewTypeName = registerToGraphViewTypeName;
            registeredPath = "";
        }
    }
}