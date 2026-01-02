using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.SystemExtensions;

namespace EncosyTower.Collections.Extensions
{
    public static class HashSetAPI
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlaps<T>(HashSet<T> a, HashSet<T> b)
        {
            return a.ReferenceEquals(b, out var bothIsNotNull)
                && bothIsNotNull
                && a.Overlaps(b);
        }
    }
}
