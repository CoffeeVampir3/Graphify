using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphBlueprintWithProperties), "Properties/String")]
    public class RuntimeStringProperty : RuntimeProperty<string> { }
}