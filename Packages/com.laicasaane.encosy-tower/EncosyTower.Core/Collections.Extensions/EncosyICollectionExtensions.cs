using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections.Extensions
{
    public static class EncosyICollectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddTo<T>(this T self, ICollection<T> collection)
            => collection.Add(self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddTo<T, TCollection>(this T self, TCollection collection)
            where TCollection : ICollection<T>
            => collection.Add(self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this ICollection<T> self)
            => self == null || self.Count < 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this IReadOnlyCollection<T> self)
            => self == null || self.Count < 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryIncreaseCapacityByFast<T>(this ICollection<T> self, int amount)
        {
            if (self is FasterList<T> fasterList)
            {
                fasterList.IncreaseCapacityBy(amount);
                return true;
            }

            if (self is List<T> list)
            {
                list.AsListFast().IncreaseCapacityBy(amount);
                return true;
            }

            if (self is HashSet<T> hashset)
            {
                hashset.EnsureCapacity(hashset.Count + amount);
                return true;
            }

            if (self is IIncreaseCapacity hasCapacity)
            {
                hasCapacity.IncreaseCapacityBy(amount);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryIncreaseCapacityToFast<T>(this ICollection<T> self, int newCapacity)
        {
            if (self is FasterList<T> fasterList)
            {
                fasterList.IncreaseCapacityTo(newCapacity);
                return true;
            }

            if (self is List<T> list)
            {
                list.AsListFast().IncreaseCapacityTo(newCapacity);
                return true;
            }

            if (self is HashSet<T> hashset)
            {
                hashset.EnsureCapacity(newCapacity);
                return true;
            }

            if (self is IIncreaseCapacity hasCapacity)
            {
                hasCapacity.IncreaseCapacityTo(newCapacity);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds a range of items to a collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="items"></param>
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
        public static void AddRange<T>(this ICollection<T> self, ReadOnlySpan<T> items)
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
                    if (coll is IIncreaseCapacity increaseCapacity)
                    {
                        increaseCapacity.IncreaseCapacityTo(coll.Count + items.Length);
                    }

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
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="items"></param>
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
        public static void AddRange<T>(this ICollection<T> self, IEnumerable<T> items)
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
                    if (coll is IIncreaseCapacity increaseCapacity && items is ICollection<T> itemsColl)
                    {
                        increaseCapacity.IncreaseCapacityTo(coll.Count + itemsColl.Count);
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
