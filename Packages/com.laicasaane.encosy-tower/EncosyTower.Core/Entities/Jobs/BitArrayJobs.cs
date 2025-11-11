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

    partial struct TrySetCapacityNativeBitArrayJob
    {
        public static TrySetCapacityNativeBitArrayJob FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<NativeBitArray>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var list) == false)
            {
                return default;
            }

            if (list.IsCreated == false || list.Length < 1)
            {
                return default;
            }

            return new TrySetCapacityNativeBitArrayJob {
                list = list,
            };
        }
    }

    partial struct ClearNativeBitArrayJob
    {
        public static ClearNativeBitArrayJob FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<NativeBitArray>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var list) == false)
            {
                return default;
            }

            if (list.IsCreated == false || list.Length < 1)
            {
                return default;
            }

            return new ClearNativeBitArrayJob {
                list = list,
            };
        }
    }

#endif

    public static class EntitiesBitArrayJobExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated(
              this TrySetCapacityNativeBitArrayJob job
            , ref SystemState state
        )
        {
            if (job.list.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScheduleIfCreated(
              this ClearNativeBitArrayJob job
            , ref SystemState state
        )
        {
            if (job.list.IsCreated)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
        }
    }
}

#endif
