using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Collections
{
    public static class EncosyICollectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddTo<T>(this T item, ICollection<T> collection)
            => collection.Add(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddTo<T, TCollection>(this T item, TCollection collection)
            where TCollection : ICollection<T>
            => collection.Add(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
            => collection == null || collection.Count < 1;
    }
}
