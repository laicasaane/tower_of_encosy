using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;

namespace EncosyTower.Vaults
{
    public sealed partial class ValueVault<TId, TValue> : IClearable
        where TId : unmanaged, IEquatable<TId>
        where TValue : struct
    {
        private readonly ConcurrentDictionary<TId, TValue> _map = new();

        public void Clear()
        {
            _map.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TId id)
            => _map.ContainsKey(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(TId id, out TValue value)
            => _map.TryRemove(id, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(TId id, out TValue value)
            => _map.TryGetValue(id, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySet(TId id, TValue value)
        {
            _map[id] = value;
            return true;
        }
    }
}
