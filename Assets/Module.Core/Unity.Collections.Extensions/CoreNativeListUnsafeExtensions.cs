#if UNITY_COLLECTIONS

using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Module.Core.Collections.Unsafe
{
    public static class CoreNativeListUnsafeExtensions
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
