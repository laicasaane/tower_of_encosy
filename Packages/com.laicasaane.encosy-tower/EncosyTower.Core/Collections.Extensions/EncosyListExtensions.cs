using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EncosyTower.Collections.Extensions
{
    public static partial class EncosyListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this IList<T> list)
            => list == null || list.Count < 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IncreaseCapacityBy<T>([NotNull] this List<T> list, int amount)
            => list.Capacity += amount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IncreaseCapacityTo<T>([NotNull] this List<T> list, int capacity)
            => list.Capacity = Math.Max(list.Capacity, capacity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>([NotNull] this List<T> list)
            => CollectionsMarshal.AsSpan(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>([NotNull] this List<T> list)
            => CollectionsMarshal.AsSpan(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListFast<T> AsListFast<T>([NotNull] this List<T> list)
            => list;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyList<T> AsReadOnlyList<T>([NotNull] this List<T> list)
            => list;

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

