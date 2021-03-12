using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterNode(typeof(GraphController), "Properties/Rect")]
    public class RuntimeRectProperty : RuntimeProperty<Rect> { }
}