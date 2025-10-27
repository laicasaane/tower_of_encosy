#if UNITY_ENTITIES

using System;
using EncosyTower.Collections;
using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EncosyTower.Jobs
{
#if LATIOS_FRAMEWORK

    partial struct ClearNativeHashSetJob<TData>
    {
        public static ClearNativeHashSetJob<TData> FromCollectionComponent<TCollectionComponent>(
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

            return new ClearNativeHashSetJob<TData> {
                set = set,
            };
        }
    }

    partial struct ClearNativeParallelHashSetJob<TData>
    {
        public static ClearNativeParallelHashSetJob<TData> FromCollectionComponent<TCollectionComponent>(
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

            return new ClearNativeParallelHashSetJob<TData> {
                set = set,
            };
        }
    }

#endif

    public static class ClearHashSetJobExtensions
    {
        public static void ScheduleIfCreated<TData>(
              this ref ClearNativeHashSetJob<TData> job
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
              this ref ClearNativeParallelHashSetJob<TData> job
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
