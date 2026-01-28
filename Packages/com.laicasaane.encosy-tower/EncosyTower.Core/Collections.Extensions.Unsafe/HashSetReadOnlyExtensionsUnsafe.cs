using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;

namespace EncosyTower.Collections.Extensions.Unsafe
{
    public static class HashSetReadOnlyExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashSet<T> GetHashSetUnsafe<T>(this HashSetReadOnly<T> set)
        {
            ThrowHelper.ThrowInvalidOperationException_ReadOnlyCollectionNotCreated(set.IsCreated);
            return set._set.Set;
        }
    }
}
