#if UNITY_COLLECTIONS

using EncosyTower.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Jobs
{
    [BurstCompile]
    public partial struct TrySetCapacityNativeListJob<TData> : IJob
        where TData : unmanaged
    {
        public int capacity;
        public NativeList<TData> list;

        [BurstCompile]
        public void Execute()
        {
            if (capacity > list.Capacity)
            {
                list.Capacity = capacity;
            }
        }
    }

    [BurstCompile]
    public partial struct ClearNativeListJob<TData> : IJob
        where TData : unmanaged
    {
        [WriteOnly] public NativeList<TData> list;

        [BurstCompile]
        public void Execute()
        {
            list.Clear();
        }
    }

    [BurstCompile]
    public partial struct ClearSharedListNativeJob<TData> : IJob
        where TData : unmanaged
    {
        [WriteOnly] public SharedListNative<TData> list;

        [BurstCompile]
        public void Execute()
        {
            list.Clear();
        }
    }
}

#endif
