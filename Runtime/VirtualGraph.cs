namespace GraphFramework.Runtime
{
    /// <summary>
    /// A "virtual" instance of a graph controller which is executable.
    /// </summary>
    public class VirtualGraph
    {
        private readonly GraphBlueprint parentGraphBlueprint;
        public readonly int virtualId;
        
        // Do not attempt to construct virtual graphs outside of the graph controller,
        // the parent graph controller must allocate the port virtualizations first.
        internal VirtualGraph(GraphBlueprint parent, int id)
        {
            parentGraphBlueprint = parent;
            virtualId = id;
        }

        ~VirtualGraph()
        {
            parentGraphBlueprint.ReleaseVirtualGraph(this);
        }
    }
}