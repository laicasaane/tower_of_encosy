#if UNITY_COLLECTIONS

using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Collections
{
    public static class EncosyNativeReferenceExtensions
    {
        /// <summary>
        /// Dispose the collection then set it to default.
        /// </summary>
        /// <param name="reference">The collection to dispose and unset</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeUnset<T>(ref this NativeReference<T> reference)
            where T : unmanaged
        {
            reference.DisposeIfCreated();
            reference = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeIfCreated<T>(this NativeReference<T> reference)
            where T : unmanaged
        {
            if (reference.IsCreated)
            {
                reference.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle DisposeIfCreated<T>(this NativeReference<T> reference, JobHandle inputDeps)
            where T : unmanaged
        {
            if (reference.IsCreated)
            {
                return reference.Dispose(inputDeps);
            }

            return inputDeps;
        }
    }
}

#endif
