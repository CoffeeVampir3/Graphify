using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphController), "Properties/Float")]
    public class RuntimeFloatProperty : RuntimeProperty<float> { }
}