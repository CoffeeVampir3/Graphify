using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphControllerWithProperties), "Properties/Float")]
    public class RuntimeFloatProperty : RuntimeProperty<float> { }
}