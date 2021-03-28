using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphBlueprintWithProperties), "Properties/Animation Curve")]
    public class RuntimeAnimationCurveProperty : RuntimeProperty<AnimationCurve> { }
}