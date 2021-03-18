using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphControllerWithProperties), "Properties/Render Texture")]
    public class RuntimeRenderTextureProperty : RuntimeProperty<RenderTexture> { }
}