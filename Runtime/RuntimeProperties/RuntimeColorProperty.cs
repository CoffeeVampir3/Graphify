using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterToGraph(typeof(GraphController), "Properties/Color")]
    public class RuntimeColorProperty : RuntimeProperty<Color> { }
}