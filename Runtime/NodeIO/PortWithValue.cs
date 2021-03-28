using System.Runtime.CompilerServices;

namespace GraphFramework
{
    internal interface PortWithValue<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool TryGetValue(int graphId, Link link, out T value);
    }
}