using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterToGraph(typeof(GraphController), "Properties/Float")]
    public class RuntimeFloatProperty : RuntimeProperty<float> { }
}