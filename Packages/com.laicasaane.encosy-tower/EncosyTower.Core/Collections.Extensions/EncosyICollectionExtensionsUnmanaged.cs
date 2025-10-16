using System;
using System.Collections.Generic;

namespace EncosyTower.Collections.Extensions
{
    public static class EncosyICollectionExtensionsUnmanaged
    {
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
        /// <item><see cref="ListFast{T}.AddRange(IEnumerable{T})"/></item>
        /// <item><see cref="SharedList{T, T}.AddRange(IEnumerable{T})"/></item>
        /// <item><see cref="FasterList{T}.AddRange(IEnumerable{T})"/></item>
        /// </list>
        /// <br/>
        /// For other types of collection, it iterates through the items and adds them one by one.
        /// </remarks>
        public static void AddRangeUnmanaged<T>(this ICollection<T> self, ReadOnlySpan<T> items)
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
            }

            if (self is null)
            {
                return;
            }

            foreach (var item in items)
            {
                self.Add(item);
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
        /// <item><see cref="SharedList{T, T}.AddRange(IEnumerable{T})"/></item>
        /// <item><see cref="FasterList{T}.AddRange(IEnumerable{T})"/></item>
        /// <item><see cref="HashSet{T}.UnionWith(IEnumerable{T})"/></item>
        /// </list>
        /// <br/>
        /// For other types of collection, it iterates through the items and adds them one by one.
        /// </remarks>
        public static void AddRangeUnmanaged<T>(this ICollection<T> self, IEnumerable<T> items)
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
            }

            if (self is null)
            {
                return;
            }

            foreach (var item in items)
            {
                self.Add(item);
            }
        }
    }
}
