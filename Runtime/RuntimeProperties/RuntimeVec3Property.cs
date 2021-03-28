using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphBlueprintWithProperties), "Properties/Vector 3")]
    public class RuntimeVec3Property : RuntimeProperty<Vector3> { }
}