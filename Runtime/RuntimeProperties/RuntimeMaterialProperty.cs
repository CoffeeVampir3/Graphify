using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterToGraph(typeof(GraphController), "Properties/Material")]
    public class RuntimeMaterialProperty : RuntimeProperty<Material> { }
}