#if UNITY_ENTITIES

using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EncosyTower.Modules.Jobs
{
#if LATIOS_FRAMEWORK

    partial struct ClearQueueJob<TData>
    {
        public static ClearQueueJob<TData> FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<NativeQueue<TData>>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var queue) == false)
            {
                return default;
            }

            if (queue.Count < 1)
            {
                return default;
            }

            return new ClearQueueJob<TData> {
                queue = queue,
            };
        }
    }

#endif

    public static class ClearQueueJobExtensions
    {
        public static void ScheduleIfCreated<TData>(this ref ClearQueueJob<TData> job, ref SystemState state)
            where TData : unmanaged
        {
            if (job.queue.IsCreated)
            {
                state.Dependency = job.ScheduleByRef(state.Dependency);
            }
        }
    }
}

#endif