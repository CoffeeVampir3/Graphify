using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphBlueprintWithProperties), "Properties/Render Texture")]
    public class RuntimeRenderTextureProperty : RuntimeProperty<RenderTexture> { }
}