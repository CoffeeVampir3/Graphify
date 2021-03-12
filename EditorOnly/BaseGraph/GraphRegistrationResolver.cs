using System;
using GraphFramework.EditorOnly.Attributes;

namespace GraphFramework.Editor
{
    public static class GraphRegistrationResolver
    {
        public static Type GetRegisteredGraphController(this CoffeeGraphView graphView)
        {
            return GetRegisteredGraphController(graphView.GetType());
        }
        
        public static Type GetRegisteredGraphController(Type graphViewType)
        {
            var attributes = graphViewType.GetCustomAttributes(false);

            foreach (var attr in attributes)
            {
                if (attr is RegisterGraphController rGraphCont)
                {
                    return rGraphCont.registerGraphControllerType;
                }
            }
            return null;
        }
    }
}