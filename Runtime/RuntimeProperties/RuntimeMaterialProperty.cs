using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphController), "Properties/Material")]
    public class RuntimeMaterialProperty : RuntimeProperty<Material> { }
}