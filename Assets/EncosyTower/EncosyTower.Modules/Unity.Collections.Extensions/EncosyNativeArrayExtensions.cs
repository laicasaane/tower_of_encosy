using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace EncosyTower.Modules.Collections
{
    public static class NativeArray
    {
        /// <summary>
        /// Create a NativeArray with memory that is not cleared and may contain garbage values.
        /// However, this method has higher performance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CreateFast<T>(int length, Allocator allocator)
            where T : unmanaged
        {
            return new NativeArray<T>(length, allocator, NativeArrayOptions.UninitializedMemory);
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

#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRange<T>(this NativeArray<T> array, Range range, T value)
            where T : struct
        {
            var (start, length) = range.GetOffsetAndLength(array.Length);
            array.AsSpan().Slice(start, length).Fill(value);
        }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ElementAtOrDefault<T>(ref this NativeArray<T> array, int index)
            where T : struct
        {
            return (uint)index < (uint)array.Length ? array[index] : default;
        }

        public static void DisposeUnset<T>(ref this NativeArray<T> array)
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
