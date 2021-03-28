using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GraphFramework.Editor
{
    public class GraphSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private GraphifyView graphView;

        public void Init(GraphifyView parentGraphView)
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
                CreateNodeSearchTreeFor(graphView.graphModel.serializedGraphBlueprint.GetType());

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
            
            //Register Stack registers to a GraphController.
            if (!typeof(CustomStackNode).IsAssignableFrom(nodeType)) return false;
            
            //Create a custom stack instance which runs the registration code... Yeah it's a bit weird but w/e
            if (!(Activator.CreateInstance(nodeType) is CustomStackNode act)) return false;
            graphView.CreateNewStack(searchTreeEntry.name, act.registeredTypes.ToArray(), 
                context.screenMousePosition);
            return true;
        }
    }
}