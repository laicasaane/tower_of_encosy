using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Vaults
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

        #region    PRESET_ID
        #endregion =========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(PresetId id)
            => Contains((Id2)id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(PresetId id)
            => TryRemove((Id2)id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(PresetId id, out TValue value)
            => TryGet((Id2)id, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySet(PresetId id, TValue value)
            => TrySet((Id2)id, value);

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
            => TypeId<T>.Value.ToId2(id);
    }
}
