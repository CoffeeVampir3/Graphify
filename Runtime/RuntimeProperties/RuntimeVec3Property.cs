using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphControllerWithProperties), "Properties/Vector 3")]
    public class RuntimeVec3Property : RuntimeProperty<Vector3> { }
}