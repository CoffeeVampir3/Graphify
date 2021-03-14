using System;
using System.Collections.Generic;
using System.Linq;
using GraphFramework.Attributes;
using UnityEditor;
using UnityEngine;

namespace GraphFramework.Editor
{
    internal static class NodeRegistrationResolver
    {
        private static readonly Type registerAttrType = typeof(RegisterTo);
        private static readonly Type rootNodeType = typeof(RootNode);

        /// <summary>
        /// Returns a list of all nodes registered to the provided graph controller type.
        /// </summary>
        public static List<Type> GetItemsRegisteredToGraph(Type graphControllerType)
        {
            var nodeList = TypeCache.GetTypesWithAttribute<RegisterTo>();
            
            //Simple, iterates through every registered node and, if they're registered to
            //our graph, or a type our graph derives from, add it to the list of registered
            //nodes and return that list.
            return (from node 
                in nodeList
                let attr = node.GetCustomAttributes(registerAttrType, false)[0] as RegisterTo
                where attr.registeredGraphType.IsAssignableFrom(graphControllerType)
                select node)
                .ToList();
        }
        
        /// <summary>
        /// Returns the first node registered as Root
        /// </summary>
        public static Type GetRegisteredRootNodeType(Type graphControllerType)
        {
            var registeredNodes = GetItemsRegisteredToGraph(graphControllerType);

            var roots = registeredNodes.Where(e => rootNodeType.IsAssignableFrom(e));
            var enumerable = roots.ToList();
            if (!enumerable.Any())
            {
                //Whoops! No RootNodes registered!
                Debug.LogError("No root registered to graph named " + graphControllerType + " make sure your root node inherits the RootNode interface." );
                return null;
            }

            if (enumerable.Count > 1)
            {
                //Whoops! Too many RootNodes registered!
                string s = enumerable.Aggregate("", (current, root) => current + ("\n" + root.Name));
                Debug.LogWarning("Warning, multiple root nodes are registered to graph controller named: " + graphControllerType + "Registered types: " + s);
            }

            return enumerable.FirstOrDefault();
        }
    }
}