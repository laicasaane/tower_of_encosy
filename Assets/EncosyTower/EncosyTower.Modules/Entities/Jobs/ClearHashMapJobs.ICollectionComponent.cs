#if UNITY_ENTITIES

using System;
using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EncosyTower.Modules.Jobs
{
#if LATIOS_FRAMEWORK

    partial struct ClearHashMapJob<TKey, TValue>
    {
        public static ClearHashMapJob<TKey, TValue> FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<NativeHashMap<TKey, TValue>>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var map) == false)
            {
                return default;
            }

            if (map.Count < 1)
            {
                return default;
            }

            return new ClearHashMapJob<TKey, TValue> {
                map = map,
            };
        }
    }

    partial struct ClearParallelHashMapJob<TKey, TValue>
    {
        public static ClearParallelHashMapJob<TKey, TValue> FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<NativeParallelHashMap<TKey, TValue>>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var map) == false)
            {
                return default;
            }

            return new ClearParallelHashMapJob<TKey, TValue> {
                map = map,
            };
        }
    }

    partial struct ClearParallelMultiHashMapJob<TKey, TValue>
    {
        public static ClearParallelMultiHashMapJob<TKey, TValue> FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<NativeParallelMultiHashMap<TKey, TValue>>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var map) == false)
            {
                return default;
            }

            return new ClearParallelMultiHashMapJob<TKey, TValue> {
                map = map,
            };
        }
    }

#endif

    public static class ClearHashMapJobExtensions
    {
        public static void ScheduleIfCreated<TKey, TValue>(
              this ref ClearHashMapJob<TKey, TValue> job
            , ref SystemState state
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (job.map.IsCreated)
            {
                state.Dependency = job.ScheduleByRef(state.Dependency);
            }
        }
    }

    public static class ClearParallelHashMapJobExtensions
    {
        public static void ScheduleIfCreated<TKey, TValue>(
              this ref ClearParallelHashMapJob<TKey, TValue> job
            , ref SystemState state
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (job.map.IsCreated)
            {
                state.Dependency = job.ScheduleByRef(state.Dependency);
            }
        }
    }

    public static class ClearParallelMultiHashMapJobExtensions
    {
        public static void ScheduleIfCreated<TKey, TValue>(
              this ref ClearParallelMultiHashMapJob<TKey, TValue> job
            , ref SystemState state
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (job.map.IsCreated)
            {
                state.Dependency = job.ScheduleByRef(state.Dependency);
            }
        }
    }
}

#endif