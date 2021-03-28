using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphBlueprintWithProperties), "Properties/Integer")]
    public class RuntimeIntProperty : RuntimeProperty<int> { }
}