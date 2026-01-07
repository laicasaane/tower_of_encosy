using System.Runtime.CompilerServices;
using EncosyTower.Buffers;

namespace EncosyTower.Collections.Unsafe
{
    public static class ArrayMapExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ManagedStrategy<ArrayMapNode<TKey>> GetKeysUnsafe<TKey, TValue>(this ArrayMap<TKey, TValue> self)
            => self._valuesInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ManagedStrategy<TValue> GetValuesUnsafe<TKey, TValue>(this ArrayMap<TKey, TValue> self)
            => self._values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MB<TValue> GetValuesUnsafe<TKey, TValue>(this ArrayMap<TKey, TValue> self, out int count)
        {
            count = self._freeValueCellIndex;
            return self._values.ToRealBuffer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TValue GetValueAtUnsafe<TKey, TValue>(this ArrayMap<TKey, TValue> self, int index)
            => ref self._values[index];
    }

    public static class ArrayMapReadOnlyExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ManagedStrategy<ArrayMapNode<TKey>>.ReadOnly GetKeysUnsafe<TKey, TValue>(this in ArrayMap<TKey, TValue>.ReadOnly self)
            => self._map._valuesInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ManagedStrategy<TValue>.ReadOnly GetValuesUnsafe<TKey, TValue>(this in ArrayMap<TKey, TValue>.ReadOnly self)
            => self._map._values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MB<TValue>.ReadOnly GetValuesUnsafe<TKey, TValue>(this in ArrayMap<TKey, TValue>.ReadOnly self, out int count)
        {
            count = self._map._freeValueCellIndex;
            return self._map._values.ToRealBuffer().AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly TValue GetValueAtUnsafe<TKey, TValue>(this in ArrayMap<TKey, TValue>.ReadOnly self, int index)
            => ref self._map._values[index];
    }
}
