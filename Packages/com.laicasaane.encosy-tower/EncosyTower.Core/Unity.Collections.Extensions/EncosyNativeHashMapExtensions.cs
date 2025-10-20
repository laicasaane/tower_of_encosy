#if UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections
{
    public static class EncosyNativeHashMapExtensions
    {
        public static void IncreaseCapacityBy<TKey, TValue>(this NativeHashMap<TKey, TValue> map, int amount)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            Checks.IsTrue(amount > 0, "amount must be greater than 0");
            IncreaseCapacityTo(map, map.Capacity + amount);
        }

        public static void IncreaseCapacityTo<TKey, TValue>(this NativeHashMap<TKey, TValue> map, int newCapacity)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (newCapacity > map.Capacity)
            {
                map.Capacity = newCapacity;
            }
        }

        public static ref NativeHashMap<TKey, TValue> NewOrClear<TKey, TValue>(
              ref this NativeHashMap<TKey, TValue> map
            , int capacity
            , AllocatorManager.AllocatorHandle allocator
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (map.IsCreated)
            {
                map.Clear();
            }
            else
            {
                map = new(capacity, allocator);
            }

            return ref map;
        }

        public static ref NativeParallelMultiHashMap<TKey, TValue> NewOrClear<TKey, TValue>(
              ref this NativeParallelMultiHashMap<TKey, TValue> map
            , int capacity
            , AllocatorManager.AllocatorHandle allocator
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (map.IsCreated)
            {
                map.Clear();
            }
            else
            {
                map = new(capacity, allocator);
            }

            return ref map;
        }

        public static void DisposeUnset<TKey, TValue>(ref this NativeHashMap<TKey, TValue> map)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (map.IsCreated)
            {
                map.Dispose();
            }

            map = default;
        }

        public static void DisposeUnset<TKey, TValue>(ref this NativeParallelMultiHashMap<TKey, TValue> map)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (map.IsCreated)
            {
                map.Dispose();
            }

            map = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<TKey, TValue>(this KVPair<TKey, TValue> kv, out TKey key, out TValue value)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            key = kv.Key;
            value = kv.Value;
        }
    }
}

#endif
