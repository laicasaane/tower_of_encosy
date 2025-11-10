#if UNITY_ENTITIES

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EncosyTower.Jobs
{
#if LATIOS_FRAMEWORK

    using Latios;

    partial struct TrySetCapacityNativeHashSetJob<TData>
    {
        public static TrySetCapacityNativeHashSetJob<TData> FromCollectionComponent<TCollectionComponent>(
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

            return new TrySetCapacityNativeHashSetJob<TData> {
                set = set,
            };
        }
    }

    partial struct TrySetCapacityNativeParallelHashSetJob<TData>
    {
        public static TrySetCapacityNativeParallelHashSetJob<TData> FromCollectionComponent<TCollectionComponent>(
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

            if (set.Count() < 1)
            {
                return default;
            }

            return new TrySetCapacityNativeParallelHashSetJob<TData> {
                set = set,
            };
        }
    }

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

    public static class EntitiesHashSetJobExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TData>(
              this TrySetCapacityNativeHashSetJob<TData> job
            , ref SystemState state
        )
            where TData : unmanaged, IEquatable<TData>
        {
            if (job.set.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TData>(
              this TrySetCapacityNativeParallelHashSetJob<TData> job
            , ref SystemState state
        )
            where TData : unmanaged, IEquatable<TData>
        {
            if (job.set.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TData>(
              this ClearNativeHashSetJob<TData> job
            , ref SystemState state
        )
            where TData : unmanaged, IEquatable<TData>
        {
            if (job.set.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TData>(
              this ClearNativeParallelHashSetJob<TData> job
            , ref SystemState state
        )
            where TData : unmanaged, IEquatable<TData>
        {
            if (job.set.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }
    }
}

#endif
