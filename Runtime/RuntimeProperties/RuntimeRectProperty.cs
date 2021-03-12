using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterToGraph(typeof(GraphController), "Properties/Rect")]
    public class RuntimeRectProperty : RuntimeProperty<Rect> { }
}