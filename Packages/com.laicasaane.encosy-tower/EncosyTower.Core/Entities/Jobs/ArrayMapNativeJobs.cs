#if UNITY_ENTITIES

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EncosyTower.Jobs
{
#if LATIOS_FRAMEWORK

    using Latios;

    partial struct TrySetCapacityArrayMapNativeJob<TKey, TValue>
    {
        public static TrySetCapacityArrayMapNativeJob<TKey, TValue> FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<ArrayMapNative<TKey, TValue>>
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

            return new TrySetCapacityArrayMapNativeJob<TKey, TValue> {
                map = map,
            };
        }
    }

    partial struct ClearArrayMapNativeJob<TKey, TValue>
    {
        public static ClearArrayMapNativeJob<TKey, TValue> FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<ArrayMapNative<TKey, TValue>>
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

            return new ClearArrayMapNativeJob<TKey, TValue> {
                map = map,
            };
        }
    }

    partial struct ClearSharedArrayMapNativeJob<TKey, TValue>
    {
        public static ClearSharedArrayMapNativeJob<TKey, TValue> FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<SharedArrayMapNative<TKey, TValue>>
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

            return new ClearSharedArrayMapNativeJob<TKey, TValue> {
                map = map,
            };
        }
    }

#endif

    public static class EntitiesArrayMapNativeJobExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TKey, TValue>(
              this TrySetCapacityArrayMapNativeJob<TKey, TValue> job
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
              this ClearArrayMapNativeJob<TKey, TValue> job
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
              this ClearSharedArrayMapNativeJob<TKey, TValue> job
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
