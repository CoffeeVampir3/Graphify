using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphController), "Properties/Render Texture")]
    public class RuntimeRenderTextureProperty : RuntimeProperty<RenderTexture> { }
}