using System;
using System.Collections.Generic;
using GraphFramework.Runtime;
using UnityEngine;

namespace GraphFramework
{
    public abstract class GraphController : ScriptableObject
    {
        [SerializeField]
        public RuntimeNode rootNode;
        [SerializeReference] 
        protected internal List<Link> links = new List<Link>();
        [NonSerialized] 
        private readonly GravestoneList<VirtualGraph> virtualizedGraphs = new GravestoneList<VirtualGraph>();

        public VirtualGraph CreateVirtualGraph()
        {
            VirtualGraph vg = new VirtualGraph(this);
            vg.virtualId = virtualizedGraphs.Add(vg);
            foreach (var link in links)
            {
                link.CreateVirtualizedLinks(vg.virtualId);
            }
            Debug.Log(vg.virtualId);

            return vg;
        }
    }
}