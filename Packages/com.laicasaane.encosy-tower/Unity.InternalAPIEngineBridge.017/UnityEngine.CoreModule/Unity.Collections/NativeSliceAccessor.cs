using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections.Internals
{
    internal unsafe readonly struct NativeSliceAccessor<T> where T : struct
    {
        public readonly NativeSlice<T> Slice;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSliceAccessor(NativeSlice<T> slice)
        {
            Slice = slice;
        }

        public byte* Buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Slice.m_Buffer;
        }

        public int Stride
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Slice.m_Stride;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Slice.m_Length;
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        public int MinIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Slice.m_MinIndex;
        }

        public int MaxIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Slice.m_MaxIndex;
        }

        public AtomicSafetyHandle Safety
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Slice.m_Safety;
        }
#endif
    }
}
