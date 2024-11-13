#if UNITY_COLLECTIONS

// ReSharper disable UnassignedField.Global

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Modules.Jobs
{
    [BurstCompile]
    public partial struct TrySetCapacityListJob<TData> : IJob
        where TData : unmanaged
    {
        public int capacity;
        public NativeList<TData> set;

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
    public partial struct ClearListJob<TData> : IJob
        where TData : unmanaged
    {
        [WriteOnly] public NativeList<TData> list;

        [BurstCompile]
        public void Execute()
        {
            list.Clear();
        }
    }
}

#endif
