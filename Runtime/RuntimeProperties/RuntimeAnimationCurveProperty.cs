using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphController), "Properties/Animation Curve")]
    public class RuntimeAnimationCurveProperty : RuntimeProperty<AnimationCurve> { }
}