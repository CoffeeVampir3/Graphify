using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GraphFramework.Editor
{
    public class CoffeeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private CoffeeGraphView graphView;

        public void Init(CoffeeGraphView parentGraphView)
        {
            graphView = parentGraphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = CoffeeGraphNodeSearchTreeProvider.
                CreateNodeSearchTreeFor(graphView.GetType());

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            //graphView.CreateNode(SearchTreeEntry.userData as Type, context.screenMousePosition);
            return true;
        }
    }
}