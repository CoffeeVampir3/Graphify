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

        /// <summary>
        /// Creates a search true using the reflection data provided by RegisterToGraph
        /// attribute.
        /// </summary>
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = GraphNodeSearchTreeProvider.
                CreateNodeSearchTreeFor(graphView.graphModel.serializedGraphController.GetType());

            return tree;
        }

        /// <summary>
        /// When an entry is selected, create that given node type and give it a default name
        /// based on the name segment of the registered path.
        /// </summary>
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (!(searchTreeEntry.userData is System.Type nodeType)) return false;
            
            if (typeof(RuntimeNode).IsAssignableFrom(nodeType))
            {
                graphView.CreateNewNode(searchTreeEntry.name, nodeType, context.screenMousePosition);
                return true;
            }
            
            //TODO:: This is a pretty shit implementation RN
            //Register Stack registers to a GraphController.
            if (!typeof(GraphController).IsAssignableFrom(nodeType)) return false;
            
            graphView.CreateNewStack(searchTreeEntry.name, null, context.screenMousePosition);
            return true; 

        }
    }
}