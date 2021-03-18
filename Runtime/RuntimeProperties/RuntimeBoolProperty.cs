using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphControllerWithProperties), "Properties/Bool")]
    public class RuntimeBoolProperty : RuntimeProperty<bool> { }
}