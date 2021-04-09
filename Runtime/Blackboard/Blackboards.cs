using System.Collections.Generic;
using GraphFramework.Runtime;

namespace GraphFramework
{
    public static class Blackboards
    {
        //Set in the graph evaluator init and runtime node eval.
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
        
        public static bool Query<T>(string queryKey, out T item)
        {
            foreach (var board in virtGraph.parentGraphBlueprint.pooledBlackboards)
            {
                if (board.TryGetValue(queryKey, out item))
                {
                    return true;
                }
            }

            item = default;
            return false;
        }
    }
}