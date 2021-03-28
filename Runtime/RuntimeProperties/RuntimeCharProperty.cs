using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphBlueprintWithProperties), "Properties/Char")]
    public class RuntimeCharProperty : RuntimeProperty<char> { }
}