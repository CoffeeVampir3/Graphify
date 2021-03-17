using System;
using System.Collections.Generic;
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

        public VirtualGraph CreateVirtualGraph()
        {
            VirtualGraph vg = new VirtualGraph(this, currentVirtualGraphIndex++);
            foreach (var link in links)
            {
                link.CreateVirtualizedLinks(vg.virtualId);
            }
            return vg;
        }
    }
}