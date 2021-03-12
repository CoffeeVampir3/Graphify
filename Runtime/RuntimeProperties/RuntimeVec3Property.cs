using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterNode(typeof(GraphController), "Properties/Vector 3")]
    public class RuntimeVec3Property : RuntimeProperty<Vector3> { }
}