using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphBlueprintWithProperties), "Properties/Float")]
    public class RuntimeFloatProperty : RuntimeProperty<float> { }
}