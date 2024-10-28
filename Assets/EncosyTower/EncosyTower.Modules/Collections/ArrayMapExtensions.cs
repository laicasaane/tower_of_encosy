using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Collections
{
    public static class ArrayMapExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<TValue> GetValues<TKey, TValue>([NotNull] this ArrayMap<TKey, TValue> self)
        {
            return self._values.ToRealBuffer().AsSpan()[..self._freeValueCellIndex];
        }
    }
}