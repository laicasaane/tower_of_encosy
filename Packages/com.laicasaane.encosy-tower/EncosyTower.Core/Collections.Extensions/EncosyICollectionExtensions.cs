using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections.Extensions
{
    public static class EncosyICollectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this ICollection<T> self)
            => self == null || self.Count < 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this IReadOnlyCollection<T> self)
            => self == null || self.Count < 1;

        /// <summary>
        /// Attempts to increase the capacity of the collection by a specified amount.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="self">The collection whose capacity is to be increased.</param>
        /// <param name="amount">The amount by which to increase the capacity.</param>
        /// <returns>
        /// <see langword="true"/> if the capacity was successfully increased; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method checks the type of the collection and attempts to increase its capacity
        /// using the most efficient method available for that type.
        /// <br/>
        /// <list type="bullet">
        /// <item><see cref="FasterList{T}.IncreaseCapacityBy(int)"/></item>
        /// <item><see cref="List{T}.AsListFast().IncreaseCapacityBy(int)"/></item>
        /// <item><see cref="HashSet{T}.EnsureCapacity(int)"/></item>
        /// <item><see cref="IIncreaseCapacity.IncreaseCapacityBy(int)"/></item>
        /// </list>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryIncreaseCapacityByFast<T>(this ICollection<T> self, int amount)
        {
            switch (self)
            {
                case FasterList<T> fasterList:
                    fasterList.IncreaseCapacityBy(amount);
                    return true;

                case List<T> list:
                    list.AsListFast().IncreaseCapacityBy(amount);
                    return true;

                case HashSet<T> hashset:
                    hashset.EnsureCapacity(hashset.Count + amount);
                    return true;

                case IIncreaseCapacity hasCapacity:
                    hasCapacity.IncreaseCapacityBy(amount);
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Attempts to increase the capacity of the collection to a specified new capacity.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="self">The collection whose capacity is to be increased.</param>
        /// <param name="newCapacity">The new capacity to which the collection should be increased to.</param>
        /// <returns>
        /// <see langword="true"/> if the capacity was successfully increased; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method checks the type of the collection and attempts to increase its capacity
        /// using the most efficient method available for that type.
        /// <br/>
        /// <list type="bullet">
        /// <item><see cref="FasterList{T}.IncreaseCapacityTo(int)"/></item>
        /// <item><see cref="List{T}.AsListFast().IncreaseCapacityTo(int)"/></item>
        /// <item><see cref="HashSet{T}.EnsureCapacity(int)"/></item>
        /// <item><see cref="IIncreaseCapacity.IncreaseCapacityTo(int)"/></item>
        /// </list>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryIncreaseCapacityToFast<T>(this ICollection<T> self, int newCapacity)
        {
            switch (self)
            {
                case FasterList<T> fasterList:
                    fasterList.IncreaseCapacityTo(newCapacity);
                    return true;

                case List<T> list:
                    list.AsListFast().IncreaseCapacityTo(newCapacity);
                    return true;

                case HashSet<T> hashset:
                    hashset.EnsureCapacity(newCapacity);
                    return true;

                case IIncreaseCapacity hasCapacity:
                    hasCapacity.IncreaseCapacityTo(newCapacity);
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Adds a range of items to a collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection and the items to be added.</typeparam>
        /// <param name="self">The collection to which the items will be added.</param>
        /// <param name="items">The items to be added to the collection.</param>
        /// <remarks>
        /// If <paramref name="self"/> can be cast to a specific collection type,
        /// it uses the most efficient method available for that type.
        /// <br/>
        /// <list type="bullet">
        /// <item><see cref="List{T}.AddRange(IEnumerable{T})"/></item>
        /// <item><see cref="FasterList{T}.AddRange(IEnumerable{T})"/></item>
        /// </list>
        /// <br/>
        /// For other types of collection, it iterates through the items and adds them one by one.
        /// </remarks>
        public static void AddRangeFast<T>(this ICollection<T> self, ReadOnlySpan<T> items)
        {
            switch (self)
            {
                case List<T> list:
                {
                    list.AsListFast().AddRange(items);
                    return;
                }

                case FasterList<T> fasterList:
                {
                    fasterList.AddRange(items);
                    return;
                }

                case ICollection<T> coll:
                {
                    coll.TryIncreaseCapacityToFast(coll.Count + items.Length);

                    foreach (var item in items)
                    {
                        coll.Add(item);
                    }

                    return;
                }
            }
        }

        /// <summary>
        /// Adds a range of items to a collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection and the items to be added.</typeparam>
        /// <param name="self">The collection to which the items will be added.</param>
        /// <param name="items">The items to be added to the collection.</param>
        /// <remarks>
        /// If <paramref name="self"/> can be cast to a specific collection type,
        /// it uses the most efficient method available for that type.
        /// <br/>
        /// <list type="bullet">
        /// <item><see cref="List{T}.AddRange(IEnumerable{T})"/></item>
        /// <item><see cref="FasterList{T}.AddRange(IEnumerable{T})"/></item>
        /// <item><see cref="HashSet{T}.UnionWith(IEnumerable{T})"/></item>
        /// </list>
        /// <br/>
        /// For other types of collection, it iterates through the items and adds them one by one.
        /// </remarks>
        public static void AddRangeFast<T>(this ICollection<T> self, IEnumerable<T> items)
        {
            if (items is null)
            {
                return;
            }

            switch (self)
            {
                case List<T> list:
                {
                    list.AddRange(items);
                    return;
                }

                case FasterList<T> fasterList:
                {
                    fasterList.AddRange(items);
                    return;
                }

                case HashSet<T> hashSet:
                {
                    hashSet.UnionWith(items);
                    return;
                }

                case ICollection<T> coll:
                {
                    switch (items)
                    {
                        case IReadOnlyCollection<T> itemsColl:
                            coll.TryIncreaseCapacityToFast(coll.Count + itemsColl.Count);
                            break;

                        case ICollection<T> itemsColl:
                            coll.TryIncreaseCapacityToFast(coll.Count + itemsColl.Count);
                            break;
                    }

                    foreach (var item in items)
                    {
                        coll.Add(item);
                    }

                    return;
                }
            }
        }
    }
}
