using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphBlueprintWithProperties), "Properties/Mesh")]
    public class RuntimeMeshProperty : RuntimeProperty<Mesh> { }
}