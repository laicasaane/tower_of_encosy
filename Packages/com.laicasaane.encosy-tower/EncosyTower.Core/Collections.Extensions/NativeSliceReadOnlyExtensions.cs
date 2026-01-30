using System.Runtime.CompilerServices;
using Unity.Collections;

namespace EncosyTower.Collections.Extensions
{
    public static class NativeSliceReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeSliceReadOnly<T> AsReadOnly<T>(this NativeSlice<T> slice)
            where T : struct
        {
            return new NativeSliceReadOnly<T>(slice);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeSliceReadOnly<T> Slice<T>(this NativeArray<T>.ReadOnly array)
            where T : struct
        {
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeSliceReadOnly<T> Slice<T>(this NativeArray<T>.ReadOnly array, int start)
            where T : struct
        {
            return new NativeSliceReadOnly<T>(array, start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeSliceReadOnly<T> Slice<T>(this NativeArray<T>.ReadOnly array, int start, int length)
            where T : struct
        {
            return new NativeSliceReadOnly<T>(array, start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeSliceReadOnly<T> Slice<T>(this NativeSliceReadOnly<T> slice)
            where T : struct
        {
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeSliceReadOnly<T> Slice<T>(this NativeSliceReadOnly<T> slice, int start)
            where T : struct
        {
            return new NativeSliceReadOnly<T>(slice, start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeSliceReadOnly<T> Slice<T>(this NativeSliceReadOnly<T> slice, int start, int length)
            where T : struct
        {
            return new NativeSliceReadOnly<T>(slice, start, length);
        }
    }
}
