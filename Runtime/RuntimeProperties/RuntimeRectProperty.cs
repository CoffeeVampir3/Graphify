using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphControllerWithProperties), "Properties/Rect")]
    public class RuntimeRectProperty : RuntimeProperty<Rect> { }
}