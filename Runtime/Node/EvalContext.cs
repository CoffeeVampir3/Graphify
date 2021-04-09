using GraphFramework.Runtime;

namespace GraphFramework
{
    internal class Context
    {
        public readonly Context parent;
        public readonly RuntimeNode contextRoot;
        public readonly VirtualGraph virtGraph;

        public Context(Context parent, RuntimeNode contextRoot, VirtualGraph virtGraph)
        {
            this.parent = parent;
            this.contextRoot = contextRoot;
            this.virtGraph = virtGraph;
        }
    }
}