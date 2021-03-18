using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphControllerWithProperties), "Properties/Color")]
    public class RuntimeColorProperty : RuntimeProperty<Color> { }
}