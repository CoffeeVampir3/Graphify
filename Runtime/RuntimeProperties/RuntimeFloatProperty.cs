using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterNode(typeof(GraphController), "Properties/Float")]
    public class RuntimeFloatProperty : RuntimeProperty<float> { }
}