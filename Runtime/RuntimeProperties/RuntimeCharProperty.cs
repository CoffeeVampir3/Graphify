using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphController), "Properties/Char")]
    public class RuntimeCharProperty : RuntimeProperty<char> { }
}