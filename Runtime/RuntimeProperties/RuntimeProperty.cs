using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    public class RuntimeProperty<T> : RuntimeNode
    {
        [Out, SerializeField] 
        protected ValuePort<T> propertyValue = new ValuePort<T>();
    }
}