using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;

namespace EncosyTower.Collections.Extensions.Unsafe
{
    public static class ListFastExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> GetListUnsafe<T>(this ListFast<T>.ReadOnly list)
        {
            ThrowHelper.ThrowInvalidOperationException_ReadOnlyCollectionNotCreated(list.IsCreated);
            return list._list.List;
        }
    }
}
