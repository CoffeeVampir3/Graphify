using System;
using System.Collections.Generic;
using System.Linq;
using GraphFramework.Attributes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GraphFramework.Editor
{
    /// <summary>
    /// Helper class to provide the node search tree for our graph view using RegisterNodeToView
    /// attribute.
    /// </summary>
    public static class GraphNodeSearchTreeProvider
    {
        private static IEnumerable<Type> GetNodesRegisteredToView(Type graphViewType)
        {
            var nodeList = TypeCache.GetTypesWithAttribute<RegisterToGraph>();
            var controllerType = GraphRegistrationResolver.GetRegisteredGraphController(graphViewType);
            if (controllerType == null)
            {
                Debug.LogError("Graph does not have a registered controller!");
                return null;
            }

            //Simple, iterates through every registered node and, if they're registered to
            //our graph, or a type our graph derives from, add it to the list of registered
            //nodes and return that list.
            return (from node 
                    in nodeList 
                    let attr = 
                        node.GetCustomAttributes(typeof(RegisterToGraph), false)[0] as RegisterToGraph 
                    where attr.registeredGraphType.IsAssignableFrom(controllerType)
                    select node)
                .ToList();
        }

        #region Node Search Tree Parser

        private static Dictionary<(string, int), SearchTreeGroupEntry> dirToGroup;
        private static Dictionary<SearchTreeGroupEntry, List<SearchTreeEntry>> groupToEntry;

        private static SearchTreeGroupEntry CreateDirectory(string directory, int depth)
        {
            (string directory, int depth) dDir = (directory, depth);
            if (dirToGroup.TryGetValue(dDir, out var searchGroup)) 
                return searchGroup;
            
            searchGroup = new SearchTreeGroupEntry(new GUIContent(directory), depth);
            dirToGroup.Add(dDir, searchGroup);
            return searchGroup;
        }

        private static void CreateEntry(Type entryNodeType, SearchTreeGroupEntry parent, string entryName, int depth)
        {
            if (!groupToEntry.TryGetValue(parent, out var entryList))
            {
                entryList = new List<SearchTreeEntry>();
                groupToEntry.Add(parent, entryList);
            }

            SearchTreeEntry nEntry = new SearchTreeEntry(new GUIContent(entryName, indentationIcon))
            {
                level = depth, userData = entryNodeType
            };

            entryList.Add(nEntry);
        }

        private static readonly Dictionary<Type, List<SearchTreeEntry>> cachedSearchTrees =
            new Dictionary<Type, List<SearchTreeEntry>>();
        private static Texture2D indentationIcon;
        /// <summary>
        /// Returns a search tree of our registered nodes for the given graph view type.
        /// This tree is cached by this function for performance.
        /// </summary>
        public static List<SearchTreeEntry> CreateNodeSearchTreeFor(Type graphViewType)
        {
            if (cachedSearchTrees.TryGetValue(graphViewType, out var tree))
                return tree;
            
            var nodeList = GetNodesRegisteredToView(graphViewType);
            dirToGroup = new Dictionary<(string directory, int depth), SearchTreeGroupEntry>();
            groupToEntry = new Dictionary<SearchTreeGroupEntry, List<SearchTreeEntry>>();

            List<SearchTreeGroupEntry> allGroups = new List<SearchTreeGroupEntry>();

            //Create our nice alignment icon, gosh it's so nice.
            //...Why the fuck is this necessary @Unity?
            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            indentationIcon.Apply();
            
            //Create top entry of our search tree.
            SearchTreeGroupEntry top = new SearchTreeGroupEntry(
                new GUIContent("Create Elements"));
            allGroups.Add(top);
            
            //Iterate through each registered node and create the layout infrastructure.
            foreach (var item in nodeList)
            {
                var attr = item.
                    GetCustomAttributes(typeof(RegisterToGraph), false)[0] as RegisterToGraph;

                Debug.Assert(attr != null, nameof(attr) + " != null");
                var split = attr.registeredPath.Split('/');
                SearchTreeGroupEntry lastGroup = top;
                for (int i = 0; i < split.Length; i++)
                {
                    string cur = split[i];

                    if (cur == "")
                        break;

                    if (i == split.Length - 1)
                    {
                        CreateEntry(item, lastGroup, cur, i+1);
                        break;
                    }
                    
                    lastGroup = CreateDirectory(cur, i+1);
                    if (!allGroups.Contains(lastGroup))
                    {
                        allGroups.Add(lastGroup);
                    }
                }
            }

            List<SearchTreeEntry> searchTree = new List<SearchTreeEntry> {top};
            //Iterate through each group and create our final search tree.
            foreach (var group in allGroups)
            {
                //Guards against top being added twice.
                if (!searchTree.Contains(group))
                {
                    searchTree.Add(group);
                }
                //For the groups with tree leaves:
                if(groupToEntry.TryGetValue(group, out var entries))
                {
                    searchTree.AddRange(entries);
                }
            }
            
            cachedSearchTrees.Add(graphViewType, searchTree);
            return searchTree;
        }
        
        #endregion
    }
}