using System;

namespace GraphFramework.Attributes
{
    public abstract class GraphRegisterable : Attribute
    {
        public Type registeredGraphType = null;
        public string registeredPath = null;
    }
}