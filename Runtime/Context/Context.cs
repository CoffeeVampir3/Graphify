using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GraphFramework.Runtime;

namespace GraphFramework
{
    public class Context
    {
        public readonly VirtualGraph virtGraph;
        private readonly Stack<RuntimeNode> contextStack = new Stack<RuntimeNode>();
        
        public Context(VirtualGraph virtGraph)
        {
            this.virtGraph = virtGraph;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count()
        {
            return contextStack.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(RuntimeNode node)
        {
            contextStack.Push(node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RuntimeNode Pop()
        {
            var m = contextStack.Pop();
            return m;
        }
    }
}