using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace EncosyTower.Collections
{
    public static class NativeArray
    {
        /// <summary>
        /// Create a NativeArray with memory that is not cleared and may contain garbage values.
        /// However, this method has higher performance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CreateFast<T>(int length, Allocator allocator)
            where T : struct
        {
            return new NativeArray<T>(length, allocator, NativeArrayOptions.UninitializedMemory);
        }

        /// <summary>
        /// Create a NativeArray from a <see cref="NativeSlice{T}"/>.
        /// </summary>
        public static NativeArray<T> CreateFrom<T>(NativeSlice<T> source, Allocator allocator)
            where T : struct
        {
            var array = CreateFast<T>(source.Length, allocator);
            source.CopyTo(array);
            return array;
        }

        /// <summary>
        /// Create a NativeArray from a <see cref="Span{T}"/>.
        /// </summary>
        public static NativeArray<T> CreateFrom<T>(Span<T> source, Allocator allocator)
            where T : struct
        {
            var array = CreateFast<T>(source.Length, allocator);
            source.CopyTo(array);
            return array;
        }

        /// <summary>
        /// Create a NativeArray from a <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        public static NativeArray<T> CreateFrom<T>(ReadOnlySpan<T> source, Allocator allocator)
            where T : struct
        {
            var array = CreateFast<T>(source.Length, allocator);
            source.CopyTo(array);
            return array;
        }

#if UNITY_COLLECTIONS

        /// <summary>
        /// Create a NativeArray with cleared memory but lower performance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> Create<T>(int length, AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
        {
            return CollectionHelper.CreateNativeArray<T>(length, allocator);
        }

        /// <summary>
        /// Create a NativeArray with memory that is not cleared and may contain garbage values.
        /// However, this method has higher performance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CreateFast<T>(int length, AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
        {
            return CollectionHelper.CreateNativeArray<T>(length, allocator, NativeArrayOptions.UninitializedMemory);
        }

        /// <summary>
        /// Create a NativeArray from a <see cref="NativeSlice{T}"/>.
        /// </summary>
        public static NativeArray<T> CreateFrom<T>(NativeSlice<T> source, AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
        {
            var array = CreateFast<T>(source.Length, allocator);
            source.CopyTo(array);
            return array;
        }

        /// <summary>
        /// Create a NativeArray from a <see cref="Span{T}"/>.
        /// </summary>
        public static NativeArray<T> CreateFrom<T>(Span<T> source, AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
        {
            var array = CreateFast<T>(source.Length, allocator);
            source.CopyTo(array);
            return array;
        }

        /// <summary>
        /// Create a NativeArray from a <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        public static NativeArray<T> CreateFrom<T>(ReadOnlySpan<T> source, AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
        {
            var array = CreateFast<T>(source.Length, allocator);
            source.CopyTo(array);
            return array;
        }

#endif
    }

    public static class EncosyNativeArrayExtensions
    {
        /// <summary>
        /// Create a NativeArray with memory that is not cleared and may contain garbage values.
        /// However, this method has higher performance.
        /// <br/>
        /// After creation, the NativeArray is assigned to <paramref name="array"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref NativeArray<T> NewFast<T>(
              ref this NativeArray<T> array, int length
            , Allocator allocator
        )
            where T : struct
        {
            array = new NativeArray<T>(length, allocator, NativeArrayOptions.UninitializedMemory);
            return ref array;
        }

#if UNITY_COLLECTIONS

        /// <summary>
        /// Create a NativeArray with cleared memory but lower performance.
        /// <br/>
        /// After creation, the NativeArray is assigned to <paramref name="array"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref NativeArray<T> New<T>(
              ref this NativeArray<T> array, int length
            , AllocatorManager.AllocatorHandle allocator
        )
            where T : unmanaged
        {
            array = CollectionHelper.CreateNativeArray<T>(length, allocator);
            return ref array;
        }

        /// <summary>
        /// Create a NativeArray with memory that is not cleared and may contain garbage values.
        /// However, this method has higher performance.
        /// <br/>
        /// After creation, the NativeArray is assigned to <paramref name="array"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref NativeArray<T> NewFast<T>(
              ref this NativeArray<T> array, int length
            , AllocatorManager.AllocatorHandle allocator
        )
            where T : unmanaged
        {
            array = CollectionHelper.CreateNativeArray<T>(length, allocator, NativeArrayOptions.UninitializedMemory);
            return ref array;
        }

#endif

        /// <summary>
        /// Fill the NativeArray with a specified value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(this NativeArray<T> array, T value)
            where T : struct
        {
            array.AsSpan().Fill(value);
        }

        /// <summary>
        /// Fill a range of the NativeArray with a specified value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(this NativeArray<T> array, Range range, T value)
            where T : struct
        {
            var (start, length) = range.GetOffsetAndLength(array.Length);
            array.AsSpan().Slice(start, length).Fill(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ElementAtOrDefault<T>(this NativeArray<T> array, int index)
            where T : struct
        {
            return (uint)index < (uint)array.Length ? array[index] : default;
        }

        public static void DisposeUnset<T>(this NativeArray<T> array)
            where T : struct
        {
            if (array.IsCreated)
            {
                array.Dispose();
            }

            array = default;
        }
    }
}
