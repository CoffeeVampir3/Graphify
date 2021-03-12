using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterToGraph(typeof(GraphController), "Properties/Char")]
    public class RuntimeCharProperty : RuntimeProperty<char> { }
}