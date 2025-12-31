using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections.Extensions.Unsafe
{
    public static class ListFastExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> GetListUnsafe<T>(this ListFast<T>.ReadOnly list)
            => list._list.List;
    }
}
