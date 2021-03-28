﻿using System;
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
        [SerializeField]
        public List<RuntimeNode> nodes = new List<RuntimeNode>();
        [NonSerialized] 
        private int currentVirtualGraphIndex = int.MinValue;
        [NonSerialized] 
        private readonly Stack<int> releasedIndices = new Stack<int>();
        [NonSerialized] 
        private bool graphInitialized = false;
        [NonSerialized] 
        internal readonly List<Link> cachedLinks = new List<Link>();

        /// <summary>
        /// An initialization you can use to frontload the graph.
        /// </summary>
        public void PrecachedInitialization()
        {
            if (graphInitialized)
                return;
            
            foreach (var node in nodes)
            {
                var type = node.GetType();
                foreach (var field in type.GetFields())
                {
                    if (typeof(BasePort).IsAssignableFrom(field.FieldType))
                    {
                        var port = field.GetValue(node) as BasePort;
                        foreach (var link in port.links)
                        {
                            cachedLinks.Add(link);
                            link.BindRemote();
                        }
                    }
                }
            }

            graphInitialized = true;
        }

        internal void InitializeId(int graphId)
        {
            if (graphInitialized)
            {
                foreach (var link in cachedLinks)
                {
                    link.Reset(graphId);
                }
                return;
            }

            ForceInitializeId(graphId);
        }

        internal void ForceInitializeId(int graphId)
        {
            foreach (var node in nodes)
            {
                var type = node.GetType();
                foreach (var field in type.GetFields())
                {
                    if (typeof(BasePort).IsAssignableFrom(field.FieldType))
                    {
                        var port = field.GetValue(node) as BasePort;
                        foreach (var link in port.links)
                        {
                            cachedLinks.Add(link);
                            link.BindRemote();
                            link.Reset(graphId);
                        }
                    }
                }
            }
        }

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
            InitializeId(vg.virtualId);
            
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