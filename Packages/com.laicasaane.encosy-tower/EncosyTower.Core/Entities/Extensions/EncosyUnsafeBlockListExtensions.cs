#if UNITY_ENTITIES && LATIOS_FRAMEWORK

using System.Runtime.CompilerServices;
using Latios.Unsafe;
using Unity.Jobs;

namespace EncosyTower.Entities
{
    public static class EncosyUnsafeBlockListExtensions
    {
        /// <summary>
        /// Dispose the collection then set it to default.
        /// </summary>
        /// <param name="list">The collection to dispose and unset</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeUnset<T>(ref this UnsafeParallelBlockList<T> list)
            where T : unmanaged
        {
            list.DisposeIfCreated();
            list = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeIfCreated<T>(this UnsafeParallelBlockList<T> list)
            where T : unmanaged
        {
            if (list.isCreated)
            {
                list.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle DisposeIfCreated<T>(this UnsafeParallelBlockList<T> list, JobHandle inputDeps)
            where T : unmanaged
        {
            if (list.isCreated)
            {
                return list.Dispose(inputDeps);
            }

            return inputDeps;
        }

        /// <summary>
        /// Dispose the collection then set it to default.
        /// </summary>
        /// <param name="list">The collection to dispose and unset</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeUnset<T>(ref this UnsafeIndexedBlockList<T> list)
            where T : unmanaged
        {
            list.DisposeIfCreated();
            list = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeIfCreated<T>(this UnsafeIndexedBlockList<T> list)
            where T : unmanaged
        {
            if (list.isCreated)
            {
                list.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle DisposeIfCreated<T>(this UnsafeIndexedBlockList<T> list, JobHandle inputDeps)
            where T : unmanaged
        {
            if (list.isCreated)
            {
                return list.Dispose(inputDeps);
            }

            return inputDeps;
        }

        /// <summary>
        /// Dispose the collection then set it to default.
        /// </summary>
        /// <param name="list">The collection to dispose and unset</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeUnset(ref this UnsafeParallelBlockList list)
        {
            list.DisposeIfCreated();
            list = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeIfCreated(this UnsafeParallelBlockList list)
        {
            if (list.isCreated)
            {
                list.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle DisposeIfCreated(this UnsafeParallelBlockList list, JobHandle inputDeps)
        {
            if (list.isCreated)
            {
                return list.Dispose(inputDeps);
            }

            return inputDeps;
        }

        /// <summary>
        /// Dispose the collection then set it to default.
        /// </summary>
        /// <param name="list">The collection to dispose and unset</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeUnset(ref this UnsafeIndexedBlockList list)
        {
            list.DisposeIfCreated();
            list = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeIfCreated(this UnsafeIndexedBlockList list)
        {
            if (list.isCreated)
            {
                list.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle DisposeIfCreated(this UnsafeIndexedBlockList list, JobHandle inputDeps)
        {
            if (list.isCreated)
            {
                return list.Dispose(inputDeps);
            }

            return inputDeps;
        }
    }
}

#endif
