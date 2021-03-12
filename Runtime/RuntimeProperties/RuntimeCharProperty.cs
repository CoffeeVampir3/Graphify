using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterNode(typeof(GraphController), "Properties/Char")]
    public class RuntimeCharProperty : RuntimeProperty<char> { }
}