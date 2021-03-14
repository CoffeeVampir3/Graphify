using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphController), "Properties/Rect")]
    public class RuntimeRectProperty : RuntimeProperty<Rect> { }
}