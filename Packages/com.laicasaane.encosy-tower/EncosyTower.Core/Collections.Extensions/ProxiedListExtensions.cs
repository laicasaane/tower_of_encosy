using System.Runtime.CompilerServices;

namespace EncosyTower.Collections.Extensions
{
    public static class ProxiedListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListFast<T>.ReadOnly AsReadOnly<T>(this ProxiedList<T> list)
            => list.IsCreated ? list._list.List : ListFast<T>.ReadOnly.Empty;
    }
}
