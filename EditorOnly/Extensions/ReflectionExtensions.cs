using System;
using System.Collections.Generic;
using System.Reflection;

namespace GraphFramework.Editor
{
    public static class ReflectionExtensions
    {
        public static List<FieldInfo> GetLocalFieldsWithAttribute<TargetAttr>(this Type t)
            where TargetAttr : Attribute
        {
            var allFields = t.GetFields(BindingFlags.Public |
                        BindingFlags.Instance | BindingFlags.NonPublic);

            var fieldList = new List<FieldInfo>();
            foreach (var f in allFields)
            {
                var attribs = f.GetCustomAttributes();

                foreach (var attr in attribs)
                {
                    if (attr.GetType() != typeof(TargetAttr)) 
                        continue;
                    fieldList.Add(f);
                    break;
                }
            }
            return fieldList;
        }

        public static System.Type[] GetGenericClassConstructorArguments(this System.Type candidateType, System.Type openGenericType)
        {
            while (true)
            {
                //We've found our target class.
                if (candidateType.IsGenericType && 
                    candidateType.GetGenericTypeDefinition() == openGenericType) 
                    return candidateType.GetGenericArguments();

                //Keep looking
                System.Type baseType = candidateType.BaseType;
                if (baseType == null) return null;
                candidateType = baseType;
            }
        }
    }
}