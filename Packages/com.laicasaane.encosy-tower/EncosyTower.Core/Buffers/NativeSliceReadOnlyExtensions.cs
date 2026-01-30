using System.Runtime.CompilerServices;
using Unity.Collections;

namespace EncosyTower.Buffers
{
    public static class NativeSliceReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeSliceReadOnly<T> Slice<T>(this NativeArray<T>.ReadOnly thisSlice)
            where T : struct
        {
            return thisSlice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeSliceReadOnly<T> Slice<T>(this NativeArray<T>.ReadOnly thisSlice, int start)
            where T : struct
        {
            return new NativeSliceReadOnly<T>(thisSlice, start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeSliceReadOnly<T> Slice<T>(this NativeArray<T>.ReadOnly thisSlice, int start, int length)
            where T : struct
        {
            return new NativeSliceReadOnly<T>(thisSlice, start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeSliceReadOnly<T> Slice<T>(this NativeSliceReadOnly<T> thisSlice)
            where T : struct
        {
            return thisSlice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeSliceReadOnly<T> Slice<T>(this NativeSliceReadOnly<T> thisSlice, int start)
            where T : struct
        {
            return new NativeSliceReadOnly<T>(thisSlice, start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeSliceReadOnly<T> Slice<T>(this NativeSliceReadOnly<T> thisSlice, int start, int length)
            where T : struct
        {
            return new NativeSliceReadOnly<T>(thisSlice, start, length);
        }
    }
}
