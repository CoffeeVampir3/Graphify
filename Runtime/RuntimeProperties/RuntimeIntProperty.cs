using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterNode(typeof(GraphController), "Properties/Integer")]
    public class RuntimeIntProperty : RuntimeProperty<int> { }
}