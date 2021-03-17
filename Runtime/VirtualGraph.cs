namespace GraphFramework.Runtime
{
    public class VirtualGraph
    {
        private GraphController parentGraphController;
        public readonly int virtualId;

        public VirtualGraph(GraphController parent, int id)
        {
            parentGraphController = parent;
            virtualId = id;
        }
    }
}