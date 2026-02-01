using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Collections.Unsafe
{
    public static class NativeSliceReadOnlyUnsafeUtility
    {
        public static unsafe void* GetUnsafePtr<T>(this NativeSliceReadOnly<T> nativeSlice) where T : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(nativeSlice.m_Safety);
#endif

            return nativeSlice._buffer;
        }

        public static unsafe void* GetUnsafeReadOnlyPtr<T>(this NativeSliceReadOnly<T> nativeSlice) where T : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(nativeSlice.m_Safety);
#endif

            return nativeSlice._buffer;
        }
    }
}
