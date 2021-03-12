using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterToGraph(typeof(GraphController), "Properties/String")]
    public class RuntimeStringProperty : RuntimeProperty<string> { }
}