#if UNITY_COLLECTIONS

using System;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections
{
    public static class EncosyNativeHashSetExtensions
    {
        public static void IncreaseCapacityBy<T>(this NativeHashSet<T> set, int amount)
            where T : unmanaged, IEquatable<T>
        {
            Checks.IsTrue(amount > 0, "amount must be greater than 0");
            IncreaseCapacityTo(set, set.Capacity + amount);
        }

        public static void IncreaseCapacityTo<T>(this NativeHashSet<T> set, int newCapacity)
            where T : unmanaged, IEquatable<T>
        {
            if (newCapacity > set.Capacity)
            {
                set.Capacity = newCapacity;
            }
        }

        public static ref NativeHashSet<T> NewOrClear<T>(
              ref this NativeHashSet<T> set
            , int capacity
            , AllocatorManager.AllocatorHandle allocator
        )
            where T : unmanaged, IEquatable<T>
        {
            if (set.IsCreated)
            {
                set.Clear();
            }
            else
            {
                set = new(capacity, allocator);
            }

            return ref set;
        }

        public static void DisposeUnset<T>(ref this NativeHashSet<T> set)
            where T : unmanaged, IEquatable<T>
        {
            if (set.IsCreated)
            {
                set.Dispose();
            }

            set = default;
        }
    }
}

#endif
