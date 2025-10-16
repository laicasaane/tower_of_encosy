using System.Runtime.CompilerServices;

namespace EncosyTower.Collections.Extensions
{
    public static class ProxiedListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyList<T> AsReadOnlyList<T>(this ProxiedList<T> list)
            => list._list.List.AsReadOnlyList();
    }
}
