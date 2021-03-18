using System;
using System.Collections.Generic;

namespace GraphFramework.Editor
{
    public abstract class CustomStackNode
    {
        internal List<Type> registeredTypes = new List<Type>();
        protected CustomStackNode()
        {
            Registration();
        }

        protected abstract void Registration();

        protected void Accepts<T>() where T : RuntimeNode
        {
            registeredTypes.Add(typeof(T));
        }
    }
}