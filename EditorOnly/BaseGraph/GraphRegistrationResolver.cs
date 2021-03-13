using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GraphFramework.Editor
{
    public static class GraphRegistrationResolver
    {
        public static List<Type> GetAllGraphControllers()
        {
            var mew = TypeCache.GetTypesDerivedFrom<GraphController>();
            foreach (var m in mew)
            {
                Debug.Log(m);
            }

            return mew.ToList();
        }
    }
}