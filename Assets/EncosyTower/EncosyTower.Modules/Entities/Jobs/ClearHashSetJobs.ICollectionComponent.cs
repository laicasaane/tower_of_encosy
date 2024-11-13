#if UNITY_ENTITIES

using System;
using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EncosyTower.Modules.Jobs
{
#if LATIOS_FRAMEWORK

    partial struct ClearHashSetJob<TData>
    {
        public static ClearHashSetJob<TData> FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<NativeHashSet<TData>>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var set) == false)
            {
                return default;
            }

            if (set.Count < 1)
            {
                return default;
            }

            return new ClearHashSetJob<TData> {
                set = set,
            };
        }
    }

    partial struct ClearParallelHashSetJob<TData>
    {
        public static ClearParallelHashSetJob<TData> FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<NativeParallelHashSet<TData>>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var set) == false)
            {
                return default;
            }

            return new ClearParallelHashSetJob<TData> {
                set = set,
            };
        }
    }

#endif

    public static class ClearHashSetJobExtensions
    {
        public static void ScheduleIfCreated<TData>(
              this ref ClearHashSetJob<TData> job
            , ref SystemState state
        )
            where TData : unmanaged, IEquatable<TData>
        {
            if (job.set.IsCreated)
            {
                state.Dependency = job.ScheduleByRef(state.Dependency);
            }
        }
    }

    public static class ClearParallelHashSetJobExtensions
    {
        public static void ScheduleIfCreated<TData>(
              this ref ClearParallelHashSetJob<TData> job
            , ref SystemState state
        )
            where TData : unmanaged, IEquatable<TData>
        {
            if (job.set.IsCreated)
            {
                state.Dependency = job.ScheduleByRef(state.Dependency);
            }
        }
    }
}

#endif
