using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GraphFramework
{
    public static class Blackboards
    {
        private static List<DataBlackboard> queryPool = 
            new List<DataBlackboard>();

        private static DataBlackboard localBb;
        private static DataBlackboard globalBb;
        public static DataBlackboard Local => localBb;
        public static DataBlackboard Global => globalBb;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetBlackboardContext(DataBlackboard loc, DataBlackboard glob, List<DataBlackboard> query)
        {
            localBb = loc;
            globalBb = glob;
            queryPool = query;
        }

        public static bool Query(string queryKey, out object item)
        {
            foreach (var board in queryPool)
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
            foreach (var board in queryPool)
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