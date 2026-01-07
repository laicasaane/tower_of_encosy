using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections
{
    public static class ArrayMapExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<TValue> GetValues<TKey, TValue>([NotNull] this ArrayMap<TKey, TValue> self)
            => self._values.ToRealBuffer().AsSpan()[..self._freeValueCellIndex];
    }

    public static class ArrayMapReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<TValue> GetValues<TKey, TValue>([NotNull] this ArrayMap<TKey, TValue>.ReadOnly self)
            => self._map._values.ToRealBuffer().AsReadOnlySpan()[..self._map._freeValueCellIndex];
    }
}
