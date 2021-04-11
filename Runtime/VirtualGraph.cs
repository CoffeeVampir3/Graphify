using System.Collections.Generic;

namespace GraphFramework.Runtime
{
    /// <summary>
    /// A "virtual" instance of a graph controller which is executable.
    /// </summary>
    public class VirtualGraph
    {
        internal readonly GraphBlueprint parentGraphBlueprint;
        internal readonly Dictionary<string, object> localBlackboardCopy;
        public readonly int virtualId;

        // Do not attempt to construct virtual graphs outside of the graph controller,
        // the parent graph controller must allocate the port virtualizations first.
        internal VirtualGraph(GraphBlueprint parent, DataBlackboard localBb, int id)
        {
            parentGraphBlueprint = parent;
            virtualId = id;
            localBlackboardCopy = localBb != null ? localBb.Copy() : null;
        }

        ~VirtualGraph()
        {
            parentGraphBlueprint.ReleaseVirtualGraph(this);
        }
    }
}