#if UNITY_ENTITIES

using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EncosyTower.Jobs
{
#if LATIOS_FRAMEWORK

    using Latios;

    partial struct ClearNativeQueueJob<TData>
    {
        public static ClearNativeQueueJob<TData> FromCollectionComponent<TCollectionComponent>(
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

            return new ClearNativeQueueJob<TData> {
                queue = queue,
            };
        }
    }

#endif

    public static class EntitiesQueueJobExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TData>(
              this ClearNativeQueueJob<TData> job
            , ref SystemState state
        )
            where TData : unmanaged
        {
            if (job.queue.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }
    }
}

#endif
