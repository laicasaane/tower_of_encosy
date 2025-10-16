using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections
{
    /// <summary>
    /// A struct that synchronizes changes made to a <see cref="ProxiedList{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ProxiedListSynchronizer<T> : IDisposable
    {
        private readonly ProxiedList<T> _list;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ProxiedListSynchronizer(ProxiedList<T> list)
        {
            _list = list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Synchronize();

        public void Synchronize()
        {
            if (_list.IsCreated)
            {
                _list.Synchronize();
            }
        }
    }
}
