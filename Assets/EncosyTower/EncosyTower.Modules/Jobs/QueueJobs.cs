#if UNITY_COLLECTIONS

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Modules.Jobs
{
    [BurstCompile]
    public partial struct ClearQueueJob<TData> : IJob
        where TData : unmanaged
    {
        [WriteOnly] public NativeQueue<TData> queue;

        [BurstCompile]
        public void Execute()
        {
            queue.Clear();
        }
    }
}

#endif
