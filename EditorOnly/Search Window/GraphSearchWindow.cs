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

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            graphView.CreateNewNode(searchTreeEntry.name, searchTreeEntry.userData as System.Type, context.screenMousePosition);
            return true;
        }
    }
}