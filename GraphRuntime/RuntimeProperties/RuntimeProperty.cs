using VisualNovelFramework.GraphFramework.Attributes;

namespace GraphFramework
{
    public class RuntimeProperty<T> : RuntimeNode
    {
        [Out] 
        protected ValuePort<T> propertyPortValue = new ValuePort<T>();

        //[SerializeField]
        //private T propertyValue;
    }
}