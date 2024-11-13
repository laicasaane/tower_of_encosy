#if UNITY_COLLECTIONS

using System.Runtime.CompilerServices;
using Unity.Collections;

namespace EncosyTower.Modules.Collections
{
    public static class EncosyNativeListExtensions
    {
        public static void NewOrClear<T>(
              ref this NativeList<T> list
            , int capacity
            , AllocatorManager.AllocatorHandle allocator
        )
            where T : unmanaged
        {
            if (list.IsCreated)
            {
                list.Clear();
            }
            else
            {
                list = new(capacity, allocator);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ElementAtOrDefault<T>(ref this NativeList<T> list, int index)
            where T : unmanaged
        {
            return (uint)index < (uint)list.Length ? list[index] : default;
        }

        public static void DisposeUnset<T>(ref this NativeList<T> list)
            where T : unmanaged
        {
            if (list.IsCreated)
            {
                list.Dispose();
            }

            list = default;
        }
    }
}

#endif
