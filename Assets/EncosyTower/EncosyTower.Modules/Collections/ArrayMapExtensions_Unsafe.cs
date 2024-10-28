using System.Runtime.CompilerServices;
using EncosyTower.Modules.Buffers;

namespace EncosyTower.Modules.Collections.Unsafe
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
    }
}
