using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphController), "Properties/Bool")]
    public class RuntimeBoolProperty : RuntimeProperty<bool> { }
}