using System;

namespace GraphFramework.EditorOnly.Attributes
{
    /// <summary>
    /// Registers a graph view to its corresponding graph controller.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterGraphController : Attribute
    {
        //Using a string is not ideal but since this is cross assembly we can't reference the type.
        public readonly Type registerGraphControllerType = null;

        /// <summary>
        /// The type name (as a string) of the graph you want to register to, this
        /// registers the node under no path, used for root nodes and specialty nodes.
        /// </summary>
        public RegisterGraphController(Type controllerType)
        {
            registerGraphControllerType = controllerType;
        }
    }
}