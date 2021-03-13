using System;
using System.Collections.Generic;
using System.Linq;
using GraphFramework.Attributes;
using UnityEditor;

namespace GraphFramework.Editor
{
    internal static class NodeRegistrationResolver
    {
        //*Technically* the nodes register to a graph controller but this bridges between
        //the two objects.
        /// <summary>
        /// Returns a list of all nodes registered to the provided graph view type.
        /// </summary>
        public static List<Type> GetItemsRegisteredToGraph<Attr>(Type graphControllerType)
        where Attr : GraphRegisterable
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

        public static Type GetRegisteredRootNodeType(Type graphControllerType)
        {
            var registeredNodes = GetItemsRegisteredToGraph<RegisterNode>(graphControllerType);

            var root = registeredNodes.FirstOrDefault(e => typeof(RootNode).IsAssignableFrom(e));
            return root;
        }
    }
}