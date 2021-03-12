using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterNode(typeof(GraphController), "Properties/Material")]
    public class RuntimeMaterialProperty : RuntimeProperty<Material> { }
}