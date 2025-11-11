#if UNITY_COLLECTIONS

using System.Runtime.CompilerServices;
using EncosyTower.Debugging;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Collections
{
    public static class EncosyNativeBitArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IncreaseCapacityBy(this NativeBitArray array, int amount)
        {
            Checks.IsTrue(amount > 0, "amount must be greater than 0");
            IncreaseCapacityTo(array, array.Capacity + amount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IncreaseCapacityTo(this NativeBitArray array, int newCapacity)
        {
            if (newCapacity > array.Capacity)
            {
                array.SetCapacity(newCapacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NewOrClear(
              ref this NativeBitArray array
            , int capacity
            , AllocatorManager.AllocatorHandle allocator
        )
        {
            if (array.IsCreated)
            {
                array.Clear();
            }
            else
            {
                array = new(capacity, allocator);
            }
        }

        /// <summary>
        /// Dispose the collection then set it to default.
        /// </summary>
        /// <param name="array">The collection to dispose and unset</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeUnset(ref this NativeBitArray array)
        {
            array.DisposeIfCreated();
            array = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeIfCreated(this NativeBitArray array)
        {
            if (array.IsCreated)
            {
                array.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle DisposeIfCreated(this NativeBitArray array, JobHandle inputDeps)
        {
            if (array.IsCreated)
            {
                return array.Dispose(inputDeps);
            }

            return inputDeps;
        }
    }
}

#endif
