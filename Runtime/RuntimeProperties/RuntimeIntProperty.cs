using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphControllerWithProperties), "Properties/Integer")]
    public class RuntimeIntProperty : RuntimeProperty<int> { }
}