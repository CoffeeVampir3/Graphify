using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphController), "Properties/Integer")]
    public class RuntimeIntProperty : RuntimeProperty<int> { }
}