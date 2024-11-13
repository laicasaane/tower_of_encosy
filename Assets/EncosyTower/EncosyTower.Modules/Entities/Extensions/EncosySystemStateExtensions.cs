#if UNITY_ENTITIES

using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;

namespace EncosyTower.Modules
{
    public static class EncosySystemStateExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CreateNativeArray<T>(ref this SystemState state, int length)
            where T : unmanaged
        {
            return CollectionHelper.CreateNativeArray<T>(
                  length
                , state.WorldUpdateAllocator
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CreateNativeArrayNotInit<T>(ref this SystemState state, int length)
            where T : unmanaged
        {
            return CollectionHelper.CreateNativeArray<T>(
                  length
                , state.WorldUpdateAllocator
                , NativeArrayOptions.UninitializedMemory
            );
        }
    }
}

#endif
