using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphControllerWithProperties), "Properties/Material")]
    public class RuntimeMaterialProperty : RuntimeProperty<Material> { }
}