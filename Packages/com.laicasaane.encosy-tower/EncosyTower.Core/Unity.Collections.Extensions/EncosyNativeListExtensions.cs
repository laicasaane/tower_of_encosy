#if UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace EncosyTower.Collections
{
    public static class EncosyNativeListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IncreaseCapacityBy<T>(this NativeList<T> list, int amount)
            where T : unmanaged
        {
            Checks.IsTrue(amount > 0, "amount must be greater than 0");
            IncreaseCapacityTo(list, list.Capacity + amount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IncreaseCapacityTo<T>(this NativeList<T> list, int newCapacity)
            where T : unmanaged
        {
            if (newCapacity > list.Capacity)
            {
                list.SetCapacity(newCapacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        /// <summary>
        /// Dispose the collection then set it to default.
        /// </summary>
        /// <param name="list">The collection to dispose and unset</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeUnset<T>(ref this NativeList<T> list)
            where T : unmanaged
        {
            list.DisposeIfCreated();
            list = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeIfCreated<T>(this NativeList<T> list)
            where T : unmanaged
        {
            if (list.IsCreated)
            {
                list.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle DisposeIfCreated<T>(this NativeList<T> list, JobHandle inputDeps)
            where T : unmanaged
        {
            if (list.IsCreated)
            {
                return list.Dispose(inputDeps);
            }

            return inputDeps;
        }

        /// <inheritdoc cref="NativeList{T}.AddRange(NativeArray{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this NativeList<T> list, ReadOnlySpan<T> items)
            where T : unmanaged
        {
            unsafe
            {
                fixed (T* ptr = items)
                {
                    list.AddRange(ptr, items.Length);
                }
            }
        }

        /// <inheritdoc cref="NativeList{T}.InsertRange(int, int)"/>
        /// <returns>
        /// A <see cref="Span{T}"/> of the inserted range.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> InsertRangeSpan<T>(this NativeList<T> list, int index, int count)
            where T : unmanaged
        {
            list.InsertRange(index, count);
            return list.AsSpan().Slice(index, count);
        }

        /// <inheritdoc cref="NativeList{T}.InsertRangeWithBeginEnd(int, int)"/>
        /// <returns>
        /// A <see cref="Span{T}"/> of the inserted range.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> InsertRangeWithBeginEndSpan<T>(this NativeList<T> list, int begin, int end)
            where T : unmanaged
        {
            list.InsertRangeWithBeginEnd(begin, end);

            var count = end - begin;

            if (count > 0)
            {
                return list.AsSpan().Slice(begin, end);
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this NativeList<T> list)
            where T : unmanaged
        {
            unsafe
            {
                return new Span<T>(list.GetUnsafePtr(), list.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeList<T> list)
            where T : unmanaged
        {
            unsafe
            {
                return new ReadOnlySpan<T>(list.GetUnsafeReadOnlyPtr(), list.Length);
            }
        }
    }
}

#endif
