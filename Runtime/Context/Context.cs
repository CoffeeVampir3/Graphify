using System.Collections.Generic;
using GraphFramework.Runtime;
using UnityEngine;

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
            Debug.Log(contextStack.Count);
            var m = contextStack.Pop();
            Debug.Log(contextStack.Count);
            return m;
        }
    }
}