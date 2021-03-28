using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphBlueprintWithProperties), "Properties/Texture 2D")]
    public class RuntimeTexture2DProperty : RuntimeProperty<Texture2D> { }
}