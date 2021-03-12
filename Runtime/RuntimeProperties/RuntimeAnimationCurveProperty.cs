using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterNode(typeof(GraphController), "Properties/Animation Curve")]
    public class RuntimeAnimationCurveProperty : RuntimeProperty<AnimationCurve> { }
}