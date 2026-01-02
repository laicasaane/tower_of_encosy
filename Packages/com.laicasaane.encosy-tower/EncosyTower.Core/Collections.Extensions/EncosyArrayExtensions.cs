using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections.Extensions
{
    public static partial class EncosyArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> ToList<T>([NotNull] this T[] self)
            => new(self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FasterList<T> ToFasterList<T>([NotNull] this T[] self)
            => new(self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] self)
            => self.AsSpan();
    }
}
