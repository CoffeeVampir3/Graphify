using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphBlueprintWithProperties), "Properties/Bounds")]
    public class RuntimeBoundsProperty : RuntimeProperty<Bounds> { }
}