using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GraphFramework.Editor
{
    public class GraphSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private CoffeeGraphView graphView;

        public void Init(CoffeeGraphView parentGraphView)
        {
            graphView = parentGraphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = GraphNodeSearchTreeProvider.
                CreateNodeSearchTreeFor(graphView.GetType());

            return tree;
        }

        /// <summary>
        /// When an entry is selected, create that given node type and give it a default name
        /// based on the name segment of the registered path.
        /// </summary>
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            graphView.CreateNewNode(searchTreeEntry.name, searchTreeEntry.userData as System.Type, context.screenMousePosition);
            return true;
        }
    }
}