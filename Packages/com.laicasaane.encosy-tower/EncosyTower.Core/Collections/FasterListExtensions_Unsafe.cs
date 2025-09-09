using System.Runtime.CompilerServices;

namespace EncosyTower.Collections.Unsafe
{
    public static class FasterListExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetBufferUnsafe<T>(this FasterList<T> list, out T[] buffer, out int count)
        {
            buffer = list._buffer;
            count = list._count;
        }
    }
}
