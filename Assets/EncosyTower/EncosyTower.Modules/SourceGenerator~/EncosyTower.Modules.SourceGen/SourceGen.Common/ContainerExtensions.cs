using System;
using System.Collections.Generic;

namespace EncosyTower.Modules.SourceGen
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Copies the last element of this list to an index. Decrements the length by 1.
        /// </summary>
        /// <remarks>Useful as a cheap way to remove elements from a list when you don't care about preserving order.</remarks>
        /// <param name="list">The list to remove from</param>
        /// <param name="index">The index to overwrite with the last element.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of bounds.</exception>
        public static void RemoveAtSwapBack<T>(this List<T> list, int index)
        {
            // swap to end
            (list[index], list[list.Count - 1]) = (list[list.Count - 1], list[index]);
            // RemoveAt is O(1) at end
            list.RemoveAt(list.Count - 1);
        }

        public static void Add<T1, T2>(this Dictionary<T1, List<T2>> dictionary, T1 key, T2 value)
        {
            if (dictionary.TryGetValue(key, out var values))
            {
                values.Add(value);
            }
            else
            {
                dictionary.Add(key, new List<T2> { value });
            }
        }
    }
}
