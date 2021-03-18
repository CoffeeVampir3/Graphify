using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphControllerWithProperties), "Properties/Char")]
    public class RuntimeCharProperty : RuntimeProperty<char> { }
}