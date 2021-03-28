using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphBlueprintWithProperties), "Properties/Material")]
    public class RuntimeMaterialProperty : RuntimeProperty<Material> { }
}