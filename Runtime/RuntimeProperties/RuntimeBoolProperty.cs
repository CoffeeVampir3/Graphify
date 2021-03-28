using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphBlueprintWithProperties), "Properties/Bool")]
    public class RuntimeBoolProperty : RuntimeProperty<bool> { }
}