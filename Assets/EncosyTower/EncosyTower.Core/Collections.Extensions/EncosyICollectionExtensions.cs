using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections
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

        /// <summary>
        /// Adds a range of items to a collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="items"></param>
        /// <remarks>
        /// If <paramref name="items"/> can be cast to a specific collection type,
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
