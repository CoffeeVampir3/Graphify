namespace GraphFramework.Runtime
{
    public class VirtualGraph
    {
        public GraphController parentGraphController;
        public int virtualId = -1;

        public VirtualGraph(GraphController parent)
        {
            parentGraphController = parent;
        }
    }
}