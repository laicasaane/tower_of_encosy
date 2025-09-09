using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Collections
{
    public static class EncosyNativeSliceExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Span<T> AsSpan<T>(this in NativeSlice<T> slice)
            where T : unmanaged
        {
            var ptr = NativeSliceUnsafeUtility.GetUnsafePtr(slice);
            return new Span<T>(ptr, slice.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ReadOnlySpan<T> AsReadOnlySpan<T>(this in NativeSlice<T> slice)
            where T : unmanaged
        {
            var ptr = NativeSliceUnsafeUtility.GetUnsafeReadOnlyPtr(slice);
            return new ReadOnlySpan<T>(ptr, slice.Length);
        }
    }
}
