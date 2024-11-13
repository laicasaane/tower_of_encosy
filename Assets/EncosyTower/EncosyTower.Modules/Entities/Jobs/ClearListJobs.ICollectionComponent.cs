#if UNITY_ENTITIES

using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EncosyTower.Modules.Jobs
{
#if LATIOS_FRAMEWORK

    partial struct ClearListJob<TData>
    {
        public static ClearListJob<TData> FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<NativeList<TData>>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var list) == false)
            {
                return default;
            }

            if (list.Length < 1)
            {
                return default;
            }

            return new ClearListJob<TData> {
                list = list,
            };
        }
    }

#endif

    public static class ClearListJobExtensions
    {
        public static void ScheduleIfCreated<TData>(this ref ClearListJob<TData> job, ref SystemState state)
            where TData : unmanaged
        {
            if (job.list.IsCreated)
            {
                state.Dependency = job.ScheduleByRef(state.Dependency);
            }
        }
    }
}

#endif