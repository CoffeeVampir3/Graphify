using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterToGraph(typeof(GraphController), "Properties/Integer")]
    public class RuntimeIntProperty : RuntimeProperty<int> { }
}