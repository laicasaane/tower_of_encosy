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

    partial struct ClearNativeHashMapJob<TKey, TValue>
    {
        public static ClearNativeHashMapJob<TKey, TValue> FromCollectionComponent<TCollectionComponent>(
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

            return new ClearNativeHashMapJob<TKey, TValue> {
                map = map,
            };
        }
    }

    partial struct ClearNativeParallelHashMapJob<TKey, TValue>
    {
        public static ClearNativeParallelHashMapJob<TKey, TValue> FromCollectionComponent<TCollectionComponent>(
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

            return new ClearNativeParallelHashMapJob<TKey, TValue> {
                map = map,
            };
        }
    }

    partial struct ClearNativeParallelMultiHashMapJob<TKey, TValue>
    {
        public static ClearNativeParallelMultiHashMapJob<TKey, TValue> FromCollectionComponent<TCollectionComponent>(
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

            return new ClearNativeParallelMultiHashMapJob<TKey, TValue> {
                map = map,
            };
        }
    }

#endif

    public static class ClearHashMapJobExtensions
    {
        public static void ScheduleIfCreated<TKey, TValue>(
              this ref ClearNativeHashMapJob<TKey, TValue> job
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
              this ref ClearNativeParallelHashMapJob<TKey, TValue> job
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
              this ref ClearNativeParallelMultiHashMapJob<TKey, TValue> job
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
