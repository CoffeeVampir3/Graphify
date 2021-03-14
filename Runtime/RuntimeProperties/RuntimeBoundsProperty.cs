using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphController), "Properties/Bounds")]
    public class RuntimeBoundsProperty : RuntimeProperty<Bounds> { }
}