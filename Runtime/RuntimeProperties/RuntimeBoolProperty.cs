using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterToGraph(typeof(GraphController), "Properties/Bool")]
    public class RuntimeBoolProperty : RuntimeProperty<bool> { }
}