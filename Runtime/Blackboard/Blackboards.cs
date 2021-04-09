using System.Collections.Generic;
using GraphFramework.Runtime;

namespace GraphFramework
{
    public static class Blackboards
    {
        internal static VirtualGraph virtGraph = null;
        public static Dictionary<string, object> Local => virtGraph.localBlackboardCopy;
        public static Dictionary<string, object> Global => virtGraph.parentGraphBlueprint.globalBlackboard.data;
        
        public static bool Query(string queryKey, out object item)
        {
            foreach (var board in virtGraph.parentGraphBlueprint.pooledBlackboards)
            {
                if (board.TryGetValue(queryKey, out item))
                {
                    return true;
                }
            }
            item = null;
            return false;
        }
        
        public static bool Query<T>(string queryKey, out object item)
        {
            foreach (var board in virtGraph.parentGraphBlueprint.pooledBlackboards)
            {
                if (board.TryGetValue(queryKey, out item))
                {
                    return true;
                }
            }
            item = null;
            return false;
        }
    }
}