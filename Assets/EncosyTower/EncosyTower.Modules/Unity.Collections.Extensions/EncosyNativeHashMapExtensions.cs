#if UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace EncosyTower.Modules.Collections
{
    public static class EncosyNativeHashMapExtensions
    {
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
