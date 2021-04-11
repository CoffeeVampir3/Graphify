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
        private static readonly Type rootNodeType = typeof(IRootNode);

        /// <summary>
        /// Returns a list of all nodes registered to the provided graph controller type.
        /// </summary>
        public static List<Type> GetItemsRegisteredToGraph<Attr>(Type graphControllerType)
        where Attr : RegistrationAttribute
        {
            var nodeList = TypeCache.GetTypesWithAttribute<Attr>();
            
            //Simple, iterates through every registered node and, if they're registered to
            //our graph, or a type our graph derives from, add it to the list of registered
            //nodes and return that list.
            return (from node 
                in nodeList
                let attr = node.GetCustomAttributes(typeof(Attr), false)[0] as Attr
                where attr.registeredGraphType.IsAssignableFrom(graphControllerType)
                select node)
                .ToList();
        }
        
        /// <summary>
        /// Returns the first node registered as Root
        /// </summary>
        public static Type GetRegisteredRootNodeType(Type graphControllerType)
        {
            var registeredNodes = GetItemsRegisteredToGraph<RegisterTo>(graphControllerType);

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
                Debug.LogError("Multiple root nodes are registered to blueprint named: " + graphControllerType + "Registered types: " + s + " you may only have one root node registered per blueprint!");
                return null;
            }

            return enumerable.FirstOrDefault();
        }
    }
}