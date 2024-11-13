using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Modules.Collections.Unsafe
{
    public static class EncosyNativeArrayUnsafeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T ElementAsUnsafeRefRW<T>(this ref NativeArray<T> array, int index)
            where T : struct
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref readonly T ElementAsUnsafeRefRO<T>(this ref NativeArray<T> array, int index)
            where T : struct
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafeReadOnlyPtr(), index);
        }

        public static unsafe void MemoryMove<T>(this ref NativeArray<T> array, int sourceStartIndex, int destinationStartIndex, int count)
            where T : struct
        {
            var sizeOf = UnsafeUtility.SizeOf<T>();
            var sizeOfInBytes = sizeOf * count;
            var ptr = (IntPtr)array.GetUnsafePtr();

            Buffer.MemoryCopy(
                  (void*)(ptr + sourceStartIndex * sizeOf)
                , (void*)(ptr + destinationStartIndex * sizeOf)
                , sizeOfInBytes
                , sizeOfInBytes
            );
        }
    }
}
