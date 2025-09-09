using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Ids;
using EncosyTower.Types;

namespace EncosyTower.Vaults
{
    public sealed partial class ValueVault<TValue> : IClearable
        where TValue : struct
    {
        private readonly Dictionary<Id2, TValue> _map = new();

        public void Clear()
        {
            _map.Clear();
        }

        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains<T>(Id<T> id)
            => Contains(ToId2(id));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove<T>(Id<T> id)
            => TryRemove(ToId2(id));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet<T>(Id<T> id, out TValue value)
            => TryGet(ToId2(id), out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySet<T>(Id<T> id, TValue value)
            => TrySet(ToId2(id), value);

        #region    ID2
        #endregion ===

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(Id2 id)
            => _map.ContainsKey(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(Id2 id)
            => _map.Remove(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(Id2 id, out TValue value)
            => _map.TryGetValue(id, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySet(Id2 id, TValue value)
        {
            _map[id] = value;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Id2 ToId2<T>(Id<T> id)
            => Type<T>.Id.ToId2(id);
    }
}
