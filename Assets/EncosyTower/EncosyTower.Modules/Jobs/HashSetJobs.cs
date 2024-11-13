#if UNITY_COLLECTIONS

// ReSharper disable UnassignedField.Global

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Modules.Jobs
{
    [BurstCompile]
    public partial struct TrySetCapacityHashSetJob<TData> : IJob
        where TData : unmanaged, IEquatable<TData>
    {
        public int capacity;
        public NativeHashSet<TData> set;

        [BurstCompile]
        public void Execute()
        {
            if (capacity > set.Capacity)
            {
                set.Capacity = capacity;
            }
        }
    }

    [BurstCompile]
    public partial struct TrySetCapacityParallelHashSetJob<TData> : IJob
        where TData : unmanaged, IEquatable<TData>
    {
        public int capacity;
        public NativeParallelHashSet<TData> set;

        [BurstCompile]
        public void Execute()
        {
            if (capacity > set.Capacity)
            {
                set.Capacity = capacity;
            }
        }
    }

    [BurstCompile]
    public partial struct ClearHashSetJob<TData> : IJob
        where TData : unmanaged, IEquatable<TData>
    {
        [WriteOnly] public NativeHashSet<TData> set;

        [BurstCompile]
        public void Execute()
        {
            set.Clear();
        }
    }

    [BurstCompile]
    public partial struct ClearParallelHashSetJob<TData> : IJob
        where TData : unmanaged, IEquatable<TData>
    {
        [WriteOnly] public NativeParallelHashSet<TData> set;

        [BurstCompile]
        public void Execute()
        {
            set.Clear();
        }
    }
}

#endif
