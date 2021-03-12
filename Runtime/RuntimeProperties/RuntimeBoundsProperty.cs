using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterNode(typeof(GraphController), "Properties/Bounds")]
    public class RuntimeBoundsProperty : RuntimeProperty<Bounds> { }
}