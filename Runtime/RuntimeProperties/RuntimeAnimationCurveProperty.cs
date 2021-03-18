using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphControllerWithProperties), "Properties/Animation Curve")]
    public class RuntimeAnimationCurveProperty : RuntimeProperty<AnimationCurve> { }
}