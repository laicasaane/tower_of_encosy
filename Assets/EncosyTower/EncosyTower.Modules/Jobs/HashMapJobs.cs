#if UNITY_COLLECTIONS

// ReSharper disable UnassignedField.Global

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Modules.Jobs
{
    [BurstCompile]
    public partial struct TrySetCapacityHashMapJob<TKey, TValue> : IJob
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
    public partial struct TrySetCapacityParallelHashMapJob<TKey, TValue> : IJob
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
    public partial struct TrySetCapacityParallelMultiHashMapJob<TKey, TValue> : IJob
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
    public partial struct ClearHashMapJob<TKey, TValue> : IJob
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
    public partial struct ClearParallelHashMapJob<TKey, TValue> : IJob
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
    public partial struct ClearParallelMultiHashMapJob<TKey, TValue> : IJob
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
