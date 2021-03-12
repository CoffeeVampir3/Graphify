using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterToGraph(typeof(GraphController), "Properties/Bounds")]
    public class RuntimeBoundsProperty : RuntimeProperty<Bounds> { }
}