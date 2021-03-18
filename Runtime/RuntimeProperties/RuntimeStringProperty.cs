using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphControllerWithProperties), "Properties/String")]
    public class RuntimeStringProperty : RuntimeProperty<string> { }
}