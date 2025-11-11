using System.Runtime.CompilerServices;
using EncosyTower.UnityExtensions;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace EncosyTower.Jobs
{
    public static class EncosyIJobParallelForTransformExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle Schedule<T>(
              this ref T jobData
            , ScheduleOnlyTransformArray transforms
            , JobHandle dependsOn = default
        )
            where T : struct, IJobParallelForTransform
        {
            return IJobParallelForTransformExtensions.Schedule(
                  jobData
                , transforms._array
                , dependsOn
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static JobHandle ScheduleReadOnly<T>(
              this T jobData
            , ScheduleOnlyTransformArray transforms
            , int batchSize
            , JobHandle dependsOn = default
        )
            where T : struct, IJobParallelForTransform
        {
            return IJobParallelForTransformExtensions.ScheduleReadOnly(
                  jobData
                , transforms._array
                , batchSize
                , dependsOn
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void RunReadOnly<T>(
              this T jobData
            , ScheduleOnlyTransformArray transforms
        )
            where T : struct, IJobParallelForTransform
        {
            IJobParallelForTransformExtensions.RunReadOnly(
                  jobData
                , transforms._array
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static JobHandle ScheduleByRef<T>(
              this ref T jobData
            , ScheduleOnlyTransformArray transforms
            , JobHandle dependsOn = default
        )
            where T : struct, IJobParallelForTransform
        {
            return IJobParallelForTransformExtensions.ScheduleByRef(
                  ref jobData
                , transforms._array
                , dependsOn
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static JobHandle ScheduleReadOnlyByRef<T>(
              this ref T jobData
            , ScheduleOnlyTransformArray transforms
            , int batchSize
            , JobHandle dependsOn = default
        )
            where T : struct, IJobParallelForTransform
        {
            return IJobParallelForTransformExtensions.ScheduleReadOnlyByRef(
                  ref jobData
                , transforms._array
                , batchSize
                , dependsOn
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void RunReadOnlyByRef<T>(this ref T jobData, ScheduleOnlyTransformArray transforms)
            where T : struct, IJobParallelForTransform
        {
            IJobParallelForTransformExtensions.RunReadOnlyByRef(
                  ref jobData
                , transforms._array
            );
        }
    }
}
