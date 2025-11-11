#if UNITY_COLLECTIONS

using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Collections
{
    public static class EncosyNativeRingQueueExtensions
    {
        /// <summary>
        /// Dispose the collection then set it to default.
        /// </summary>
        /// <param name="queue">The collection to dispose and unset</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeUnset<T>(ref this NativeRingQueue<T> queue)
            where T : unmanaged
        {
            queue.DisposeIfCreated();
            queue = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeIfCreated<T>(this NativeRingQueue<T> queue)
            where T : unmanaged
        {
            if (queue.IsCreated)
            {
                queue.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle DisposeIfCreated<T>(this NativeRingQueue<T> queue, JobHandle inputDeps)
            where T : unmanaged
        {
            if (queue.IsCreated)
            {
                return queue.Dispose(inputDeps);
            }

            return inputDeps;
        }
    }
}

#endif
