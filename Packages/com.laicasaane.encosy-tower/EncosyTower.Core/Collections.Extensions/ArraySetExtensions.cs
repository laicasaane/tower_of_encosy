using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections
{
    public static class ArraySetExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> GetValues<T>([NotNull] this ArraySet<T> self)
            => self._values.ToRealBuffer().AsSpan()[..self._freeValueCellIndex];
    }

    public static class ArraySetReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> GetValues<T>([NotNull] this ArraySet<T>.ReadOnly self)
            => self._set._values.ToRealBuffer().AsReadOnlySpan()[..self._set._freeValueCellIndex];
    }
}
