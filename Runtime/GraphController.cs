using System;
using System.Collections.Generic;
using System.Linq;
using GraphFramework.Runtime;
using UnityEngine;

namespace GraphFramework
{
    public abstract class GraphController : ScriptableObject
    {
        [SerializeField, HideInInspector]
        public RuntimeNode rootNode;
        [SerializeReference, HideInInspector] 
        protected internal List<Link> links = new List<Link>();
        [NonSerialized] 
        private int currentVirtualGraphIndex = int.MinValue;
        [NonSerialized] 
        private readonly Stack<int> releasedIndices = new Stack<int>();
        /// <summary>
        /// Creates and initializes a virtualized graph.
        /// </summary>
        /// <returns>The initialized graph.</returns>
        public VirtualGraph CreateVirtualGraph()
        {
            int nextGraphIndex = currentVirtualGraphIndex;
            if (releasedIndices.Any())
            {
                nextGraphIndex = releasedIndices.Pop();
            }
            else
            {
                currentVirtualGraphIndex++;
            }

            VirtualGraph vg = new VirtualGraph(this, nextGraphIndex);
            foreach (var link in links)
            {
                link.CreateVirtualizedLinks(vg.virtualId);
            }
            return vg;
        }
        
        /// <summary>
        /// Returns a graph ID to the pool, this is so that the ValuePort dictionary memory can be recycled.
        /// </summary>
        internal void ReleaseVirtualGraph(VirtualGraph vg)
        {
            releasedIndices.Push(vg.virtualId);
        }
    }
}