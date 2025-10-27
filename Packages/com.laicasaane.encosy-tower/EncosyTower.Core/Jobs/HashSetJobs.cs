#if UNITY_COLLECTIONS

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Jobs
{
    [BurstCompile]
    public partial struct TrySetCapacityNativeHashSetJob<TData> : IJob
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
    public partial struct TrySetCapacityNativeParallelHashSetJob<TData> : IJob
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
    public partial struct ClearNativeHashSetJob<TData> : IJob
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
    public partial struct ClearNativeParallelHashSetJob<TData> : IJob
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
