#if UNITY_COLLECTIONS

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Jobs
{
    [BurstCompile]
    public partial struct TrySetCapacityNativeHashMapJob<TKey, TValue> : IJob
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        public int capacity;
        public NativeHashMap<TKey, TValue> map;

        [BurstCompile]
        public void Execute()
        {
            if (capacity > map.Capacity)
            {
                map.Capacity = capacity;
            }
        }
    }

    [BurstCompile]
    public partial struct TrySetCapacityNativeParallelHashMapJob<TKey, TValue> : IJob
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        public int capacity;
        public NativeParallelHashMap<TKey, TValue> map;

        [BurstCompile]
        public void Execute()
        {
            if (capacity > map.Capacity)
            {
                map.Capacity = capacity;
            }
        }
    }

    [BurstCompile]
    public partial struct TrySetCapacityNativeParallelMultiHashMapJob<TKey, TValue> : IJob
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        public int capacity;
        public NativeParallelMultiHashMap<TKey, TValue> map;

        [BurstCompile]
        public void Execute()
        {
            if (capacity > map.Capacity)
            {
                map.Capacity = capacity;
            }
        }
    }

    [BurstCompile]
    public partial struct ClearNativeHashMapJob<TKey, TValue> : IJob
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        [WriteOnly] public NativeHashMap<TKey, TValue> map;

        [BurstCompile]
        public void Execute()
        {
            map.Clear();
        }
    }

    [BurstCompile]
    public partial struct ClearNativeParallelHashMapJob<TKey, TValue> : IJob
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        [WriteOnly] public NativeParallelHashMap<TKey, TValue> map;

        [BurstCompile]
        public void Execute()
        {
            map.Clear();
        }
    }

    [BurstCompile]
    public partial struct ClearNativeParallelMultiHashMapJob<TKey, TValue> : IJob
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        [WriteOnly] public NativeParallelMultiHashMap<TKey, TValue> map;

        [BurstCompile]
        public void Execute()
        {
            map.Clear();
        }
    }
}

#endif
