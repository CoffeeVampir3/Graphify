using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterToGraph(typeof(GraphController), "Properties/Render Texture")]
    public class RuntimeRenderTextureProperty : RuntimeProperty<RenderTexture> { }
}