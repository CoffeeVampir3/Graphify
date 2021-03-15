using System;
using System.Linq;
using System.Reflection;
using GraphFramework.EditorOnly.Attributes;
using UnityEditor;

namespace GraphFramework.Editor
{
    public static class ViewRegistrationResolver
    {
        public static bool TryGetCustom(Type forType, out Type registeredType)
        {
            registeredType = null;
            var viewTypeList = TypeCache.GetTypesWithAttribute<RegisterViewFor>();
            foreach (var viewType in viewTypeList)
            {
                var attribs = viewType.GetCustomAttributes();
                var rvForAttrib = attribs.FirstOrDefault(
                    (e) => e is RegisterViewFor) as RegisterViewFor;
                if (rvForAttrib == null)
                    return false;
                
                if (forType.IsAssignableFrom(rvForAttrib.runtimeNodeType))
                {
                    registeredType = viewType;
                    return true;
                }
            }
            
            
            return false;
        }
    }
}