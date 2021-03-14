using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphController), "Properties/String")]
    public class RuntimeStringProperty : RuntimeProperty<string> { }
}