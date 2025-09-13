using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Collections.Unsafe
{
    public static partial class EncosyListExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpanUnsafe<T>([NotNull] this List<T> list)
            => UnsafeUtility.As<List<T>, FasterList<T>>(ref list)._buffer.AsSpan(0, list.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpanUnsafe<T>([NotNull] this List<T> list)
            => UnsafeUtility.As<List<T>, FasterList<T>>(ref list)._buffer.AsSpan(0, list.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<T> AsMemoryUnsafe<T>([NotNull] this List<T> list)
            => UnsafeUtility.As<List<T>, FasterList<T>>(ref list)._buffer.AsMemory(0, list.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<T> AsReadOnlyMemoryUnsafe<T>([NotNull] this List<T> list)
            => UnsafeUtility.As<List<T>, FasterList<T>>(ref list)._buffer.AsMemory(0, list.Count);
    }
}
