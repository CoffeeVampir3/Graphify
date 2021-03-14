using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphController), "Properties/Color")]
    public class RuntimeColorProperty : RuntimeProperty<Color> { }
}