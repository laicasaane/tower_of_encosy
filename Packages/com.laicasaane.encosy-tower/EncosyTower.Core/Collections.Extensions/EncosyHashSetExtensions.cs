using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashSetReadOnly<T> AsReadOnly<T>(this HashSet<T> set)
            => set;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExceptWith<T>([NotNull] this HashSet<T> set, HashSetReadOnly<T> other)
            => set.ExceptWith(other._set.Set);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntersectWith<T>([NotNull] this HashSet<T> set, HashSetReadOnly<T> other)
            => set.IntersectWith(other._set.Set);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SymmetricExceptWith<T>([NotNull] this HashSet<T> set, HashSetReadOnly<T> other)
            => set.SymmetricExceptWith(other._set.Set);
    }
}
