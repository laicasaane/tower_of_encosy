using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections.Internals
{
    internal unsafe readonly struct NativeArrayReadOnlyAccessor<T> where T : struct
    {
        public readonly NativeArray<T>.ReadOnly Array;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArrayReadOnlyAccessor(NativeArray<T>.ReadOnly array)
        {
            Array = array;
        }

        public void* Buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Array.m_Buffer;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Array.m_Length;
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        public AtomicSafetyHandle Safety
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Array.m_Safety;
        }
#endif
    }
}
