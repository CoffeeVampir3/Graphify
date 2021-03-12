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
        //*Technically* the nodes register to a graph controller but this bridges between
        //the two objects.
        /// <summary>
        /// Returns a list of all nodes registered to the provided graph view type.
        /// </summary>
        public static List<Type> GetItemsRegisteredToGraph<Attr>(Type graphViewType)
        where Attr : GraphRegisterable
        {
            var nodeList = TypeCache.GetTypesWithAttribute<Attr>();
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
                        node.GetCustomAttributes(typeof(Attr), false)[0] as Attr 
                    where attr.registeredGraphType.IsAssignableFrom(controllerType)
                    select node)
                .ToList();
        }

        public static Type GetRegisteredRootNodeType(Type graphViewType)
        {
            var registeredNodes = GetItemsRegisteredToGraph<RegisterNode>(graphViewType);

            var root = registeredNodes.FirstOrDefault(e => typeof(RootNode).IsAssignableFrom(e));
            return root;
        }
    }
}