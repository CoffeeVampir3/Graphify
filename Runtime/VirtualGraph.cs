namespace GraphFramework.Runtime
{
    /// <summary>
    /// A "virtual" instance of a graph controller which is executable.
    /// </summary>
    public class VirtualGraph
    {
        private readonly GraphController parentGraphController;
        public readonly int virtualId;
        
        // Do not attempt to construct virtual graphs outside of the graph controller,
        // the parent graph controller must allocate the port virtualizations first.
        internal VirtualGraph(GraphController parent, int id)
        {
            parentGraphController = parent;
            virtualId = id;
        }

        ~VirtualGraph()
        {
            parentGraphController.ReleaseVirtualGraph(this);
        }
    }
}