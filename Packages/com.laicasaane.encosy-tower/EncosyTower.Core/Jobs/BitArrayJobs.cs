#if UNITY_COLLECTIONS

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Jobs
{
    [BurstCompile]
    public partial struct TrySetCapacityNativeBitArrayJob : IJob
    {
        public int capacity;
        public NativeBitArray list;

        [BurstCompile]
        public void Execute()
        {
            if (capacity > list.Capacity)
            {
                list.SetCapacity(capacity);
            }
        }
    }

    [BurstCompile]
    public partial struct ClearNativeBitArrayJob : IJob
    {
        [WriteOnly] public NativeBitArray list;

        [BurstCompile]
        public void Execute()
        {
            list.Clear();
        }
    }
}

#endif
