using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterTo(typeof(GraphControllerWithProperties), "Properties/Mesh")]
    public class RuntimeMeshProperty : RuntimeProperty<Mesh> { }
}