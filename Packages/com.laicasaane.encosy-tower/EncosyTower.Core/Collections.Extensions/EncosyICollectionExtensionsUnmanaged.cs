using System;
using System.Collections.Generic;

namespace EncosyTower.Collections.Extensions
{
    public static class EncosyICollectionExtensionsUnmanaged
    {
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
        /// <item><see cref="ListFast{T}.AddRange(IEnumerable{T})"/></item>
        /// <item><see cref="SharedList{T, T}.AddRange(IEnumerable{T})"/></item>
        /// <item><see cref="FasterList{T}.AddRange(IEnumerable{T})"/></item>
        /// </list>
        /// <br/>
        /// For other types of collection, it iterates through the items and adds them one by one.
        /// </remarks>
        public static void AddRangeFastUnmanaged<T>(this ICollection<T> self, ReadOnlySpan<T> items)
            where T : unmanaged
        {
            if (items.Length < 1)
            {
                return;
            }

            switch (self)
            {
                case List<T> list:
                {
                    list.AsListFast().AddRange(items);
                    return;
                }

                case SharedList<T> sharedList:
                {
                    sharedList.AddRange(items);
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
                        self.Add(item);
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
        /// <item><see cref="SharedList{T, T}.AddRange(IEnumerable{T})"/></item>
        /// <item><see cref="FasterList{T}.AddRange(IEnumerable{T})"/></item>
        /// <item><see cref="HashSet{T}.UnionWith(IEnumerable{T})"/></item>
        /// </list>
        /// <br/>
        /// For other types of collection, it iterates through the items and adds them one by one.
        /// </remarks>
        public static void AddRangeFastUnmanaged<T>(this ICollection<T> self, IEnumerable<T> items)
            where T : unmanaged
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

                case SharedList<T> sharedList:
                {
                    sharedList.AddRange(items);
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
                        self.Add(item);
                    }

                    break;
                }
            }
        }
    }
}
