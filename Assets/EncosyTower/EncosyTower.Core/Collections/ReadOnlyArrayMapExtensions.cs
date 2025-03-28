using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections
{
    public static class ReadOnlyArrayMapExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<TValue> GetValues<TKey, TValue>([NotNull] this ReadOnlyArrayMap<TKey, TValue> self)
        {
            return self._map.GetValues();
        }
    }
}
