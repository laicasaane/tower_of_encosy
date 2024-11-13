#if UNITY_COLLECTIONS

using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Modules.Collections.Unsafe
{
    public static class EncosyNativeListUnsafeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T ElementAsUnsafeRefRW<T>(ref this NativeList<T> list, int index)
            where T : unmanaged
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(list.GetUnsafePtr(), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref readonly T ElementAsUnsafeRefRO<T>(ref this NativeList<T> list, int index)
            where T : unmanaged
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(list.GetUnsafeReadOnlyPtr(), index);
        }
    }
}

#endif
