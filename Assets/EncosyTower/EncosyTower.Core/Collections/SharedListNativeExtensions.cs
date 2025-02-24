using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections
{
    public static class SharedListNativeExtensions
    {
        public static bool Contains<T, TNative>([NotNull] this in SharedListNative<T, TNative> self, TNative item)
            where T : unmanaged
            where TNative : unmanaged, IEquatable<TNative>
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

        public static bool Contains<T, TNative>([NotNull] this in SharedListNative<T, TNative> self, in TNative item)
            where T : unmanaged
            where TNative : unmanaged, IEquatable<TNative>
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

        public static bool Remove<T, TNative>([NotNull] this in SharedListNative<T, TNative> self, TNative item)
            where T : unmanaged
            where TNative : unmanaged, IEquatable<TNative>
        {
            self.VersionRW++;

            var index = IndexOf(self, item);

            if (index < 0)
                return false;

            if (index < --self.CountRW)
            {
                self._buffer.MemoryCopyUnsafe(index + 1, index, self.Count - index);
            }

            return true;
        }

        public static bool Remove<T, TNative>([NotNull] this in SharedListNative<T, TNative> self, in TNative item)
            where T : unmanaged
            where TNative : unmanaged, IEquatable<TNative>
        {
            self.VersionRW++;

            var index = IndexOf(self, item);

            if (index < 0)
                return false;

            if (index < --self.CountRW)
            {
                self._buffer.MemoryCopyUnsafe(index + 1, index, self.Count - index);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>([NotNull] this in SharedListNative<T, TNative> self, TNative item, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<TNative>
            => BinarySearch(self, 0, self.Count, item, comparer);

        public static int BinarySearch<T, TNative, TComparer>([NotNull] this in SharedListNative<T, TNative> self, int index, int count, TNative item, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<TNative>
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(self.Count - index >= count, "index and count do not denote a valid range in the SharedList<T, TNative>");

            return self.AsNativeSlice().BinarySearch(item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>([NotNull] this in SharedListNative<T, TNative> self, in TNative item, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<TNative>
            => BinarySearch(self, 0, self.Count, in item, comparer);

        public static int BinarySearch<T, TNative, TComparer>([NotNull] this in SharedListNative<T, TNative> self, int index, int count, in TNative item, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<TNative>
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(self.Count - index >= count, "index and count do not denote a valid range in the SharedList<T, TNative>");

            return self.AsNativeSlice().BinarySearch(item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this in SharedListNative<T, TNative> self, TNative item)
            where T : unmanaged
            where TNative : unmanaged, IEquatable<TNative>
            => IndexOf(self, item, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this in SharedListNative<T, TNative> self, TNative item, int index)
            where T : unmanaged
            where TNative : unmanaged, IEquatable<TNative>
            => IndexOf(self, item, index, self.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this in SharedListNative<T, TNative> self, TNative item, int index, int count)
            where T : unmanaged
            where TNative : unmanaged, IEquatable<TNative>
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(index + count <= self.Count, "index and count do not specify a valid section in the SharedList<T, TNative>");

            var result = MemoryExtensions.IndexOf(self.AsReadOnlySpan().Slice(index, count), item);
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this in SharedListNative<T, TNative> self, in TNative item)
            where T : unmanaged
            where TNative : unmanaged, IEquatable<TNative>
            => IndexOf(self, in item, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this in SharedListNative<T, TNative> self, in TNative item, int index)
            where T : unmanaged
            where TNative : unmanaged, IEquatable<TNative>
            => IndexOf(self, in item, index, self.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this in SharedListNative<T, TNative> self, in TNative item, int index, int count)
            where T : unmanaged
            where TNative : unmanaged, IEquatable<TNative>
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(index + count <= self.Count, "index and count do not specify a valid section in the SharedList<T, TNative>");

            var result = MemoryExtensions.IndexOf(self.AsReadOnlySpan().Slice(index, count), item);
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative, TComparer>([NotNull] this in SharedListNative<T, TNative> self, TNative item, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<TNative>
            => MemoryExtensions.BinarySearch(self.AsReadOnlySpan(), item, comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative, TComparer>([NotNull] this in SharedListNative<T, TNative> self, in TNative item, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<TNative>
            => MemoryExtensions.BinarySearch(self.AsReadOnlySpan(), item, comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T, TNative, TComparer>([NotNull] this in SharedListNative<T, TNative> self, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<TNative>
            => Sort(self, 0, self.Count, comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T, TNative, TComparer>([NotNull] this in SharedListNative<T, TNative> self, int index, int count, TComparer comparer)
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : unmanaged, IComparer<TNative>
        {
            Checks.IsTrue(index >= 0, "'index' must be non-negative number");
            Checks.IsTrue(count >= 0, "'count' must be non-negative number");
            Checks.IsTrue(self.Count - index >= count, "Invalid offset length");

            self.VersionRW++;

            if (count > 1)
            {
                self.AsNativeSlice().Slice(index, count).Sort(comparer);
            }
        }
    }
}
