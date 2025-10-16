using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.Collections.Extensions.Unsafe
{
    public static class ProxiedListExtensionsUnsafe
    {
        /// <summary>
        /// Gets the internal <see cref="List{T}"/> of a <see cref="ProxiedList{T}"/> without any safety checks.
        /// </summary>
        /// <returns>
        /// A <see cref="ProxiedListSynchronizer{T}"/> to synchronize changes from <paramref name="result"/>.
        /// </returns>
        /// <remarks>
        /// This method is unsafe because it exposes the internal <see cref="List{T}"/> directly,
        /// allowing modifications that may bypass any proxy logic or validation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProxiedListSynchronizer<T> GetListUnsafe<T>(this ProxiedList<T> self, out List<T> result)
        {
            result = self._list.List;
            return new(self);
        }

        /// <summary>
        /// Gets the internal <see cref="FasterList{T}"/> of a <see cref="ProxiedList{T}"/> without any safety checks.
        /// </summary>
        /// <returns>
        /// A <see cref="ProxiedListSynchronizer{T}"/> to synchronize changes from <paramref name="result"/>.
        /// </returns>
        /// <remarks>
        /// This method is unsafe because it exposes the internal <see cref="FasterList{T}"/> directly,
        /// allowing modifications that may bypass any proxy logic or validation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProxiedListSynchronizer<T> GetListProxyUnsafe<T>(this ProxiedList<T> self, out Option<FasterList<T>> result)
        {
            if (self._proxy is FasterListExtensions.FasterListProxy<T> proxy)
            {
                result = Option.Some(proxy._list);
                return new(self);
            }

            result = Option.None;
            return new(self);
        }

        /// <summary>
        /// Gets the internal <see cref="SharedList{T}"/> of a <see cref="ProxiedList{T}"/> without any safety checks.
        /// </summary>
        /// <returns>
        /// A <see cref="ProxiedListSynchronizer{T}"/> to synchronize changes from <paramref name="result"/>.
        /// </returns>
        /// <remarks>
        /// This method is unsafe because it exposes the internal <see cref="SharedList{T}"/> directly,
        /// allowing modifications that may bypass any proxy logic or validation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProxiedListSynchronizer<T> GetListProxyUnsafe<T>(this ProxiedList<T> self, out Option<SharedList<T>> result)
            where T : unmanaged
        {
            if (self._proxy is SharedListExtensions.SharedListProxy<T, T, SharedList<T>> proxy)
            {
                result = Option.Some(proxy._list);
                return new(self);
            }

            result = Option.None;
            return new(self);
        }

        /// <summary>
        /// Gets the internal <see cref="SharedList{T, TNative}"/> of a <see cref="ProxiedList{T}"/> without any safety checks.
        /// </summary>
        /// <returns>
        /// A <see cref="ProxiedListSynchronizer{T}"/> to synchronize changes from <paramref name="result"/>.
        /// </returns>
        /// <remarks>
        /// This method is unsafe because it exposes the internal <see cref="SharedList{T, TNative}"/> directly,
        /// allowing modifications that may bypass any proxy logic or validation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProxiedListSynchronizer<T> GetListProxyUnsafe<T, TNative>(this ProxiedList<T> self, out Option<SharedList<T, TNative>> result)
            where T : unmanaged
            where TNative : unmanaged
        {
            if (self._proxy is SharedListExtensions.SharedListProxy<T, TNative, SharedList<T, TNative>> proxy)
            {
                result = Option.Some(proxy._list);
                return new(self);
            }

            result = Option.None;
            return new(self);
        }
    }
}
