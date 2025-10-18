#if UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections.Extensions
{
    public static class ArrayMapNativeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<TValue> GetValues<TKey, TValue>(this in ArrayMapNative<TKey, TValue> self)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            return self._values.ToRealBuffer().AsSpan()[..self._freeValueCellIndex.Value];
        }
    }
}

#endif
