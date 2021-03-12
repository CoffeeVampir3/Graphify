using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterToGraph(typeof(GraphController), "Properties/Mesh")]
    public class RuntimeMeshProperty : RuntimeProperty<Mesh> { }
}