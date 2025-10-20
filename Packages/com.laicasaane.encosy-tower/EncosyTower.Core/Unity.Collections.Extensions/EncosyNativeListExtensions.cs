#if UNITY_COLLECTIONS

using System.Runtime.CompilerServices;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections
{
    public static class EncosyNativeListExtensions
    {
        public static void IncreaseCapacityBy<T>(this NativeList<T> list, int amount)
            where T : unmanaged
        {
            Checks.IsTrue(amount > 0, "amount must be greater than 0");
            IncreaseCapacityTo(list, list.Capacity + amount);
        }

        public static void IncreaseCapacityTo<T>(this NativeList<T> list, int newCapacity)
            where T : unmanaged
        {
            if (newCapacity > list.Capacity)
            {
                list.SetCapacity(newCapacity);
            }
        }

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
        public static T ElementAtOrDefault<T>(this NativeList<T> list, int index)
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
