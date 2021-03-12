using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterNode(typeof(GraphController), "Properties/Mesh")]
    public class RuntimeMeshProperty : RuntimeProperty<Mesh> { }
}