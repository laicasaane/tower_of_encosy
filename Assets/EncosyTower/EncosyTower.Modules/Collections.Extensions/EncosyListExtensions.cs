namespace EncosyTower.Modules.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    public static partial class EncosyListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this IList<T> list)
            => list == null || list.Count < 1;

        public static void AddRange<T>(
              [NotNull] this List<T> list
            , ReadOnlyMemory<T> collection
        )
        {
            var span = collection.Span;
            var length = span.Length;

            if (length < 1)
            {
                return;
            }

            list.Capacity = list.Count + length;

            for (var i = 0; i < length; i++)
            {
                list.Add(span[i]);
            }
        }

        public static void AddRangeTo<T>(
              this IEnumerable<T> src
            , ref List<T> dest
        )
        {
            if (src == null)
            {
                return;
            }

            if (dest == null)
            {
                dest = new(src);
            }
            else
            {
                dest.AddRange(src);
            }
        }
    }
}

namespace EncosyTower.Modules.Collections.Unsafe
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using Unity.Collections.LowLevel.Unsafe;

    public static partial class CoreListExtensions
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
