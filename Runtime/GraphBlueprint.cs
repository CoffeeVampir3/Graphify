using System;
using System.Collections.Generic;
using System.Linq;
using GraphFramework.Runtime;
using Graphify.Runtime;
using UnityEngine;

namespace GraphFramework
{
    public abstract class GraphBlueprint : ScriptableObject, HasAssetGuid
    {
        [SerializeField, HideInInspector]
        public RuntimeNode rootNode;
        [SerializeReference] 
        public GraphBlueprint parentGraph = null;
        [SerializeField] 
        public DataBlackboard dataBlackboard;
        [SerializeReference] 
        public List<GraphBlueprint> childGraphs = new List<GraphBlueprint>();
        [SerializeField]
        public List<RuntimeNode> nodes = new List<RuntimeNode>();
        [NonSerialized] 
        private int currentVirtualGraphIndex = int.MinValue;
        [NonSerialized] 
        private readonly Stack<int> releasedIndices = new Stack<int>();
        [NonSerialized] 
        private bool graphInitialized = false;
        [NonSerialized] 
        private List<Link> cachedLinks;
        [field: SerializeField]
        public string AssetGuid { get; set; }
        public string editorGraphGuid;

        public void Bootstrap(string editorGuid)
        {
            editorGraphGuid = editorGuid;
            AssetGuid = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// An initialization you can use to frontload the graph.
        /// </summary>
        public void Precache()
        {
            if (graphInitialized)
                return;
            
            cachedLinks = new List<Link>(100);
            BuildCache(0, false, ref cachedLinks);
        }

        private void Cache(int graphId, bool reset, ref List<Link> links)
        {
            for (var index = nodes.Count - 1; index >= 0; index--)
            {
                var node = nodes[index];
                if (node == null)
                {
                    nodes.RemoveAt(index);
                    continue;
                }
                
                var type = node.GetType();
                foreach (var field in type.GetFields())
                {
                    if (!typeof(BasePort).IsAssignableFrom(field.FieldType))
                        continue;

                    var port = field.GetValue(node) as BasePort;
                    if (port == null)
                    {
                    #if UNITY_EDITOR
                        Debug.LogError(node?.name + " was skipped during graph + " + this.name +
                                       " because the port relating to " + field.Name + " was null.");
                    #endif
                        continue;
                    }

                    foreach (var link in port.links)
                    {
                        links.Add(link);
                        link.BindRemote();
                        if (reset)
                            link.Reset(graphId);
                    }
                }
            }
        }

        private void BuildCache(int graphId, bool reset, ref List<Link> links)
        {
            Cache(graphId, reset, ref links);
            foreach (var child in childGraphs)
            {
                child.Cache(graphId, reset, ref links);
                child.BuildCache(graphId, reset, ref links);
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

            cachedLinks = new List<Link>(100);
            BuildCache(graphId, true, ref cachedLinks);
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