using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterNode(typeof(GraphController), "Properties/Render Texture")]
    public class RuntimeRenderTextureProperty : RuntimeProperty<RenderTexture> { }
}