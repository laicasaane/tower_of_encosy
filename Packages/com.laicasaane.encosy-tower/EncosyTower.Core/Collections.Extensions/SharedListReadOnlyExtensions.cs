using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections.Extensions
{
    public static class SharedListReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T, TNative>(this SharedList<T, TNative>.ReadOnly self, T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            var items = self.AsReadOnlySpan();
            var length = items.Length;
            return length > 0 && MemoryExtensions.IndexOf(items, item) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T, TNative>(this SharedList<T, TNative>.ReadOnly self, in T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            var items = self.AsReadOnlySpan();
            var length = items.Length;
            return length > 0 && MemoryExtensions.IndexOf(items, item) >= 0;
        }
        
        public static bool Contains<T, TNative, TComparer>(
              this SharedList<T, TNative>.ReadOnly self
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IEqualityComparer<T>
        {
            var items = self.AsReadOnlySpan();
            var length = items.Length;

            for (var index = 0; index < length; index++)
            {
                ref readonly var item2 = ref items[index];

                if (comparer.Equals(item, item2))
                    return true;
            }

            return false;
        }
        
        public static bool Contains<T, TNative, TComparer>(
              this SharedList<T, TNative>.ReadOnly self
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IEqualityComparer<T>
        {
            var items = self.AsReadOnlySpan();
            var length = items.Length;

            for (var index = 0; index < length; index++)
            {
                ref readonly var item2 = ref items[index];

                if (comparer.Equals(item, item2))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>(
              this SharedList<T, TNative>.ReadOnly self
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            return BinarySearch(self, 0, self.Count, item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>(
              this SharedList<T, TNative>.ReadOnly self
            , int index
            , int count
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(
                  self.Count - index >= count
                , "index and count do not denote a valid range in the SharedList<T, TNative>.ReadOnly"
            );

            return self.AsReadOnlySpan().BinarySearch(item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>(
              this SharedList<T, TNative>.ReadOnly self
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            return BinarySearch(self, 0, self.Count, in item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>(
              this SharedList<T, TNative>.ReadOnly self
            , int index
            , int count
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(
                  self.Count - index >= count
                , "index and count do not denote a valid range in the SharedList<T, TNative>.ReadOnly"
            );

            return self.AsReadOnlySpan().BinarySearch(item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>(this SharedList<T, TNative>.ReadOnly self, T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
            => IndexOf(self, item, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>(this SharedList<T, TNative>.ReadOnly self, T item, int index)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
            => IndexOf(self, item, index, self.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>(this SharedList<T, TNative>.ReadOnly self, T item, int index, int count)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(
                  index + count <= self.Count
                , "index and count do not specify a valid section in the SharedList<T, TNative>.ReadOnly"
            );

            var result = MemoryExtensions.IndexOf(self.AsReadOnlySpan().Slice(index, count), item);
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>(this SharedList<T, TNative>.ReadOnly self, in T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
            => IndexOf(self, in item, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>(this SharedList<T, TNative>.ReadOnly self, in T item, int index)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
            => IndexOf(self, in item, index, self.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>(this SharedList<T, TNative>.ReadOnly self, in T item, int index, int count)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(
                  index + count <= self.Count
                , "index and count do not specify a valid section in the SharedList<T, TNative>.ReadOnly"
            );

            var result = MemoryExtensions.IndexOf(self.AsReadOnlySpan().Slice(index, count), item);
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative, TComparer>(
              this SharedList<T, TNative>.ReadOnly self
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            return MemoryExtensions.BinarySearch(self.AsReadOnlySpan(), item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative, TComparer>(
              this SharedList<T, TNative>.ReadOnly self
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            return MemoryExtensions.BinarySearch(self.AsReadOnlySpan(), item, comparer);
        }
    }
}
