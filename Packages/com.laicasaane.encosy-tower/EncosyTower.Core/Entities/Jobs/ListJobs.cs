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

    partial struct TrySetCapacityNativeListJob<TData>
    {
        public static TrySetCapacityNativeListJob<TData> FromCollectionComponent<TCollectionComponent>(
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

            return new TrySetCapacityNativeListJob<TData> {
                list = list,
            };
        }
    }

    partial struct ClearNativeListJob<TData>
    {
        public static ClearNativeListJob<TData> FromCollectionComponent<TCollectionComponent>(
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

            return new ClearNativeListJob<TData> {
                list = list,
            };
        }
    }

    partial struct ClearSharedListNativeJob<TData>
    {
        public static ClearSharedListNativeJob<TData> FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<SharedListNative<TData>>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var list) == false)
            {
                return default;
            }

            if (list.Count < 1)
            {
                return default;
            }

            return new ClearSharedListNativeJob<TData> {
                list = list,
            };
        }
    }

#endif

    public static class EntitiesListJobExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TData>(
              this TrySetCapacityNativeListJob<TData> job
            , ref SystemState state
        )
            where TData : unmanaged
        {
            if (job.list.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TData>(
              this ClearNativeListJob<TData> job
            , ref SystemState state
        )
            where TData : unmanaged
        {
            if (job.list.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated<TData>(
              this ClearSharedListNativeJob<TData> job
            , ref SystemState state
        )
            where TData : unmanaged
        {
            if (job.list.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }
    }
}

#endif
