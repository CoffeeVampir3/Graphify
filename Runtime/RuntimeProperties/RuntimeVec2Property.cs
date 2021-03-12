using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterToGraph(typeof(GraphController), "Properties/Vector 2")]
    public class RuntimeVec2Property : RuntimeProperty<Vector2> { }
}