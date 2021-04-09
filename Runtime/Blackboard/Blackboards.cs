using System.Runtime.CompilerServices;
using GraphFramework.Runtime;

namespace GraphFramework
{
    public static class Blackboards
    {
        private static GraphBlueprint blueprint;
        public static DataBlackboard Local => blueprint.localBlackboard;
        public static DataBlackboard Global => blueprint.globalBlackboard;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetBlackboardContext(VirtualGraph virtGraph)
        {
            blueprint = virtGraph.parentGraphBlueprint;
        }

        public static bool Query(string queryKey, out object item)
        {
            foreach (var board in blueprint.pooledBlackboards)
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
            foreach (var board in blueprint.pooledBlackboards)
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