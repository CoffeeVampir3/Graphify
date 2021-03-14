using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphController), "Properties/Texture 2D")]
    public class RuntimeTexture2DProperty : RuntimeProperty<Texture2D> { }
}