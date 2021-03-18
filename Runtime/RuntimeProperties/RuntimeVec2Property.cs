using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphControllerWithProperties), "Properties/Vector 2")]
    public class RuntimeVec2Property : RuntimeProperty<Vector2> { }
}