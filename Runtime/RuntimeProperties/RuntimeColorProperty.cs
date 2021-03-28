using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphBlueprintWithProperties), "Properties/Color")]
    public class RuntimeColorProperty : RuntimeProperty<Color> { }
}