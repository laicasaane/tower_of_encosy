#if UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Collections.Unsafe
{
    public static class EncosyNativeReferenceExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T ValueAsUnsafeRefRW<T>(this NativeReference<T> reference)
            where T : unmanaged
        {
            return ref UnsafeUtility.AsRef<T>(reference.GetUnsafePtr());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref readonly T ValueAsUnsafeRefRO<T>(this NativeReference<T> reference)
            where T : unmanaged
        {
            return ref UnsafeUtility.AsRef<T>(reference.GetUnsafePtr());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> AsSpan<T>(this NativeReference<T> reference)
            where T : unmanaged
        {
            return new Span<T>(reference.GetUnsafePtr(), 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeReference<T> reference)
            where T : unmanaged
        {
            return new ReadOnlySpan<T>(reference.GetUnsafeReadOnlyPtr(), 1);
        }
    }
}

#endif
