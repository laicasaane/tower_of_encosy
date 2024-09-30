#if UNITY_COLLECTIONS

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Module.Core.Collections
{
    public static class NativeArrayMapExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<TValue> GetValues<TKey, TValue>([NotNull] this ref NativeArrayMap<TKey, TValue> self)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            return self._values.ToRealBuffer().AsSpan()[..self._freeValueCellIndex.Value];
        }
    }
}

#endif
