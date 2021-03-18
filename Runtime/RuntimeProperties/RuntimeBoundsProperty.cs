using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphControllerWithProperties), "Properties/Bounds")]
    public class RuntimeBoundsProperty : RuntimeProperty<Bounds> { }
}