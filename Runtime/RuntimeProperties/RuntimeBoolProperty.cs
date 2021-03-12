using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterNode(typeof(GraphController), "Properties/Bool")]
    public class RuntimeBoolProperty : RuntimeProperty<bool> { }
}