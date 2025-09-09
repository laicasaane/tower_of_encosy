using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections
{
    public static class EncosyIEnumerableExtensions
    {
        /// <summary>
        /// Try to get the count of <paramref name="self"/> if it is either:
        /// <list type="bullet">
        /// <item><see cref="IReadOnlyCollection{T}"/></item>
        /// <item><see cref="ICollection{T}"/></item>
        /// </list>
        /// </summary>
        /// <returns>
        /// True if the count was retrieved successfully, false otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetCountFast<T>(this IEnumerable<T> self, out int count)
        {
            if (self is IReadOnlyCollection<T> readonlyCollection)
            {
                count = readonlyCollection.Count;
                return true;
            }

            if (self is ICollection<T> collection)
            {
                count = collection.Count;
                return true;
            }

            count = 0;
            return false;
        }
    }
}
