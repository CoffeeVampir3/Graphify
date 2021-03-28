using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphBlueprintWithProperties), "Properties/Rect")]
    public class RuntimeRectProperty : RuntimeProperty<Rect> { }
}