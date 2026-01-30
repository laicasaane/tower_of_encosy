using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections.Internals
{
    internal unsafe readonly struct NativeArrayAccessor<T> where T : struct
    {
        public readonly NativeArray<T> Array;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArrayAccessor(NativeArray<T> array)
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

        public int MinIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Array.m_MinIndex;
        }

        public int MaxIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Array.m_MaxIndex;
        }

        public AtomicSafetyHandle Safety
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Array.m_Safety;
        }
    }
}
