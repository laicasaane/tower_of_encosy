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

    partial struct TrySetCapacityNativeHashMapJob<TKey, TValue>
    {
        public static TrySetCapacityNativeHashMapJob<TKey, TValue> FromCollectionComponent<TCollectionComponent>(
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

            if (map.IsCreated == false || map.Count < 1)
            {
                return default;
            }

            return new TrySetCapacityNativeHashMapJob<TKey, TValue> {
                map = map,
            };
        }
    }

    partial struct TrySetCapacityNativeParallelHashMapJob<TKey, TValue>
    {
        public static TrySetCapacityNativeParallelHashMapJob<TKey, TValue> FromCollectionComponent<TCollectionComponent>(
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

            if (map.IsCreated == false || map.Count() < 1)
            {
                return default;
            }

            return new TrySetCapacityNativeParallelHashMapJob<TKey, TValue> {
                map = map,
            };
        }
    }

    partial struct TrySetCapacityNativeParallelMultiHashMapJob<TKey, TValue>
    {
        public static TrySetCapacityNativeParallelMultiHashMapJob<TKey, TValue> FromCollectionComponent<TCollectionComponent>(
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

            if (map.IsCreated == false || map.Count() < 1)
            {
                return default;
            }

            return new TrySetCapacityNativeParallelMultiHashMapJob<TKey, TValue> {
                map = map,
            };
        }
    }

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

            if (map.IsCreated == false || map.Count < 1)
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

            if (map.IsCreated == false || map.Count() < 1)
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

            if (map.IsCreated == false || map.Count() < 1)
            {
                return default;
            }

            return new ClearNativeParallelMultiHashMapJob<TKey, TValue> {
                map = map,
            };
        }
    }

#endif

    public static class EntitiesHashMapJobExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TKey, TValue>(
              this TrySetCapacityNativeHashMapJob<TKey, TValue> job
            , ref SystemState state
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (job.map.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TKey, TValue>(
              this TrySetCapacityNativeParallelHashMapJob<TKey, TValue> job
            , ref SystemState state
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (job.map.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TKey, TValue>(
              this TrySetCapacityNativeParallelMultiHashMapJob<TKey, TValue> job
            , ref SystemState state
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (job.map.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TKey, TValue>(
              this ClearNativeHashMapJob<TKey, TValue> job
            , ref SystemState state
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (job.map.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TKey, TValue>(
              this ClearNativeParallelHashMapJob<TKey, TValue> job
            , ref SystemState state
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (job.map.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TKey, TValue>(
              this ClearNativeParallelMultiHashMapJob<TKey, TValue> job
            , ref SystemState state
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (job.map.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }
    }
}

#endif
