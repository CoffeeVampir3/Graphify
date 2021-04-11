using System.Collections.Generic;
using GraphFramework.Runtime;

namespace GraphFramework
{
    public class Context
    {
        public readonly VirtualGraph virtGraph;
        private readonly Stack<RuntimeNode> contextStack = new Stack<RuntimeNode>();
        public int Count => contextStack.Count;

        public Context(RuntimeNode rootNode, VirtualGraph virtGraph)
        {
            this.virtGraph = virtGraph;
            Push(rootNode);
        }

        public void Push(RuntimeNode node)
        {
            contextStack.Push(node);
        }

        public RuntimeNode Pop()
        {
            var m = contextStack.Pop();
            return m;
        }
    }
}