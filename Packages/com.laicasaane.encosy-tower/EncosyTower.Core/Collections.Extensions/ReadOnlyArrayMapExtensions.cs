using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections.Extensions
{
    public static class ReadOnlyArrayMapExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<TValue> GetValues<TKey, TValue>([NotNull] this ArrayMap<TKey, TValue>.ReadOnly self)
        {
            return self._map.GetValues();
        }
    }
}
