using System;
using System.Collections.Generic;

namespace GraphFramework.Editor
{
    public abstract class CustomStackNode
    {
        internal readonly List<Type> registeredTypes = new List<Type>();
        protected CustomStackNode()
        {
            //This is a virtual call in the constructor, but the order of operations isint an issue
            //for this case.
            Registration();
        }

        protected abstract void Registration();

        protected void Accepts<T>() where T : RuntimeNode
        {
            registeredTypes.Add(typeof(T));
        }
    }
}