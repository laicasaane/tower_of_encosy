#if UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Modules.Collections.Unsafe
{
    public static class EncosyNativeReferenceUnsafeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T ValueAsUnsafeRefRW<T>(ref this NativeReference<T> reference)
            where T : unmanaged
        {
            return ref UnsafeUtility.AsRef<T>(reference.GetUnsafePtr());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref readonly T ValueAsUnsafeRefRO<T>(ref this NativeReference<T> reference)
            where T : unmanaged
        {
            return ref UnsafeUtility.AsRef<T>(reference.GetUnsafePtr());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> AsSpan<T>(ref this NativeReference<T> reference)
            where T : unmanaged
        {
            return new Span<T>(reference.GetUnsafePtr(), 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(ref this NativeReference<T> reference)
            where T : unmanaged
        {
            return new ReadOnlySpan<T>(reference.GetUnsafeReadOnlyPtr(), 1);
        }
    }
}

#endif
