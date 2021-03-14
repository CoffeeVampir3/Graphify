using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphController), "Properties/Mesh")]
    public class RuntimeMeshProperty : RuntimeProperty<Mesh> { }
}