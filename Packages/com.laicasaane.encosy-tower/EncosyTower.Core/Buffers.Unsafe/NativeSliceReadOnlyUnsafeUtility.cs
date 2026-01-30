using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Buffers.Unsafe
{
    public static class NativeSliceReadOnlyUnsafeUtility
    {
        public static AtomicSafetyHandle GetAtomicSafetyHandle<T>(NativeSliceReadOnly<T> slice) where T : struct
        {
            return slice.m_Safety;
        }

        public static void SetAtomicSafetyHandle<T>(ref NativeSliceReadOnly<T> slice, AtomicSafetyHandle safety) where T : struct
        {
            slice.m_Safety = safety;
        }

        public static unsafe void* GetUnsafePtr<T>(this NativeSliceReadOnly<T> nativeSlice) where T : struct
        {
            AtomicSafetyHandle.CheckWriteAndThrow(nativeSlice.m_Safety);
            return nativeSlice._buffer;
        }

        public static unsafe void* GetUnsafeReadOnlyPtr<T>(this NativeSliceReadOnly<T> nativeSlice) where T : struct
        {
            AtomicSafetyHandle.CheckReadAndThrow(nativeSlice.m_Safety);
            return nativeSlice._buffer;
        }
    }
}
