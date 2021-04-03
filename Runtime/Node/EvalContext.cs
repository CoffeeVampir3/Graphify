namespace GraphFramework
{
    internal class Context
    {
        public readonly Context parent;
        public readonly RuntimeNode contextRoot;
        public readonly int graphId;

        public Context(Context parent, RuntimeNode contextRoot, int graphId)
        {
            this.parent = parent;
            this.contextRoot = contextRoot;
            this.graphId = graphId;
        }
    }
}