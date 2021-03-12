using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterNode(typeof(GraphController), "Properties/String")]
    public class RuntimeStringProperty : RuntimeProperty<string> { }
}