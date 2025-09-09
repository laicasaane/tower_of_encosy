using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections
{
    public static class SharedListExtensions
    {
        public static bool Contains<T, TNative>([NotNull] this SharedList<T, TNative> self, T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            var items = self.AsReadOnlySpan();
            var length = items.Length;

            for (var index = 0; index < length; index++)
            {
                ref readonly var item2 = ref items[index];

                if (item.Equals(item2))
                    return true;
            }

            return false;
        }

        public static bool Contains<T, TNative>([NotNull] this SharedList<T, TNative> self, in T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            var items = self.AsReadOnlySpan();
            var length = items.Length;

            for (var index = 0; index < length; index++)
            {
                ref readonly var item2 = ref items[index];

                if (item.Equals(item2))
                    return true;
            }

            return false;
        }

        public static bool Remove<T, TNative>([NotNull] this SharedList<T, TNative> self, T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            self._version.ValueRW++;

            var index = IndexOf(self, item);

            if (index < 0)
                return false;

            if (index < --self._count.ValueRW)
            {
                var buffer = self._buffer.AsManagedArray();
                Array.Copy(buffer, index + 1, buffer, index, self._count.ValueRO - index);
            }

            return true;
        }

        public static bool Remove<T, TNative>([NotNull] this SharedList<T, TNative> self, in T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            self._version.ValueRW++;

            var index = self.AsSpan().BinarySearch(item, Comparer<T>.Default);

            if ((uint)index >= (uint)self._count.ValueRO)
                return false;

            if (index < --self._count.ValueRW)
            {
                var buffer = self._buffer.AsManagedArray();
                Array.Copy(buffer, index + 1, buffer, index, self._count.ValueRO - index);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>([NotNull] this SharedList<T, TNative> self, T item, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<T>
            => BinarySearch(self, 0, self._count.ValueRO, item, comparer);

        public static int BinarySearch<T, TNative, TComparer>([NotNull] this SharedList<T, TNative> self, int index, int count, T item, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<T>
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(self._count.ValueRO - index >= count, "index and count do not denote a valid range in the SharedList<T, TNative>");

            return self.AsReadOnlySpan().BinarySearch(item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>([NotNull] this SharedList<T, TNative> self, in T item, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<T>
            => BinarySearch(self, 0, self._count.ValueRO, in item, comparer);

        public static int BinarySearch<T, TNative, TComparer>([NotNull] this SharedList<T, TNative> self, int index, int count, in T item, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<T>
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(self._count.ValueRO - index >= count, "index and count do not denote a valid range in the SharedList<T, TNative>");

            return self.AsReadOnlySpan().BinarySearch(item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this SharedList<T, TNative> self, T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
            => IndexOf(self, item, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this SharedList<T, TNative> self, T item, int index)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
            => IndexOf(self, item, index, self._count.ValueRO);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this SharedList<T, TNative> self, T item, int index, int count)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(index + count <= self._count.ValueRO, "index and count do not specify a valid section in the SharedList<T, TNative>");

            var result = MemoryExtensions.IndexOf(self.AsReadOnlySpan().Slice(index, count), item);
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this SharedList<T, TNative> self, in T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
            => IndexOf(self, in item, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this SharedList<T, TNative> self, in T item, int index)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
            => IndexOf(self, in item, index, self._count.ValueRO);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this SharedList<T, TNative> self, in T item, int index, int count)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(index + count <= self._count.ValueRO, "index and count do not specify a valid section in the SharedList<T, TNative>");

            var result = MemoryExtensions.IndexOf(self.AsReadOnlySpan().Slice(index, count), item);
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative, TComparer>([NotNull] this SharedList<T, TNative> self, T item, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<T>
            => MemoryExtensions.BinarySearch(self.AsReadOnlySpan(), item, comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative, TComparer>([NotNull] this SharedList<T, TNative> self, in T item, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<T>
            => MemoryExtensions.BinarySearch(self.AsReadOnlySpan(), item, comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T, TNative, TComparer>([NotNull] this SharedList<T, TNative> self, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<T>
            => Sort(self, 0, self._count.ValueRO, comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T, TNative, TComparer>([NotNull] this SharedList<T, TNative> self, int index, int count, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<T>
        {
            Checks.IsTrue(index >= 0, "'index' must be non-negative number");
            Checks.IsTrue(count >= 0, "'count' must be non-negative number");
            Checks.IsTrue(self._count.ValueRO - index >= count, "Invalid offset length");

            self._version.ValueRW++;
            ArraySortHelper<T, TComparer>.Sort(self.AsSpan().Slice(index, count), comparer);
        }
    }
}
