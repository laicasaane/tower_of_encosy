using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Collections.Unsafe
{
    public static class NativeArrayUnsafe
    {
        /// <summary>
        /// Convert an existing <see cref="Span{T}"/> to a NativeArray.
        /// </summary>
        public static NativeArray<T> ConvertFrom<T>(Span<T> source, Allocator allocator)
            where T : unmanaged
        {
            return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray(source, allocator);
        }
    }

    public static class EncosyNativeArrayExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T ElementAsUnsafeRefRW<T>(this NativeArray<T> array, int index)
            where T : struct
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref readonly T ElementAsUnsafeRefRO<T>(this NativeArray<T> array, int index)
            where T : struct
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafeReadOnlyPtr(), index);
        }

        public static unsafe void MemoryCopyUnsafe<T>(
              this NativeArray<T> array
            , int sourceIndex
            , int destinationIndex
            , int length
        )
            where T : struct
        {
            var arrayLength = array.Length;
            var sizeOf = UnsafeUtility.SizeOf<T>();
            var ptr = (IntPtr)array.GetUnsafePtr();

            Buffer.MemoryCopy(
                  (void*)(ptr + sourceIndex * sizeOf)
                , (void*)(ptr + destinationIndex * sizeOf)
                , (arrayLength - length - destinationIndex) * sizeOf
                , (arrayLength - sourceIndex) * sizeOf
            );
        }

        public static unsafe void MemoryCopyUnsafe<T>(
              this NativeArray<T> source
            , int sourceIndex
            , NativeArray<T> destination
            , int destinationIndex
            , int length
        ) where T : struct
        {
            var sizeOfT = UnsafeUtility.SizeOf<T>();
            var dstPtr = (IntPtr)destination.GetUnsafePtr();
            var srcPtr = (IntPtr)source.GetUnsafePtr();

            UnsafeUtility.MemCpy(
                  destination: (void*)(dstPtr + destinationIndex * sizeOfT)
                , source: (void*)(srcPtr + sourceIndex * sizeOfT)
                , size: length * sizeOfT
            );
        }

        public static unsafe void MemoryCopyUnsafeWithoutChecks<T>(
              this NativeArray<T> source
            , int sourceIndex
            , NativeArray<T> destination
            , int destinationIndex
            , int length
        ) where T : struct
        {
            var sizeOfT = UnsafeUtility.SizeOf<T>();
            var srcPtr = (IntPtr)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(source);
            var dstPtr = (IntPtr)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(destination);

            UnsafeUtility.MemCpy(
                  destination: (void*)(dstPtr + destinationIndex * sizeOfT)
                , source: (void*)(srcPtr + sourceIndex * sizeOfT)
                , size: length * sizeOfT
            );
        }
    }
}
