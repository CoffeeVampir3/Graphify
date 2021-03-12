using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterToGraph(typeof(GraphController), "Properties/Animation Curve")]
    public class RuntimeAnimationCurveProperty : RuntimeProperty<AnimationCurve> { }
}