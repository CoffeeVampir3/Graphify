using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterNode(typeof(GraphController), "Properties/Color")]
    public class RuntimeColorProperty : RuntimeProperty<Color> { }
}