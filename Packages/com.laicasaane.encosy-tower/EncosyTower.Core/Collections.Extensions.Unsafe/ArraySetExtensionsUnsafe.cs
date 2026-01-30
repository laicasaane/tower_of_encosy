using System.Runtime.CompilerServices;
using EncosyTower.Buffers;

namespace EncosyTower.Collections.Unsafe
{
    public static class ArraySetExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ManagedStrategy<ArrayMapNode<T>> GetNodesUnsafe<T>(this ArraySet<T> self)
            => self._valuesInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ManagedStrategy<T> GetItemsUnsafe<T>(this ArraySet<T> self)
            => self._values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MB<T> GetItemsUnsafe<T>(this ArraySet<T> self, out int count)
        {
            count = self._freeValueCellIndex;
            return self._values.ToRealBuffer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetItemAtUnsafe<T>(this ArraySet<T> self, int index)
            => ref self._values[index];
    }

    public static class ArraySetReadOnlyExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ManagedStrategy<ArrayMapNode<T>>.ReadOnly GetNodesUnsafe<T>(this in ArraySet<T>.ReadOnly self)
            => self._set._valuesInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ManagedStrategy<T>.ReadOnly GetItemsUnsafe<T>(this in ArraySet<T>.ReadOnly self)
            => self._set._values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MB<T>.ReadOnly GetItemsUnsafe<T>(this in ArraySet<T>.ReadOnly self, out int count)
        {
            count = self._set._freeValueCellIndex;
            return self._set._values.ToRealBuffer().AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly T GetItemAtUnsafe<T>(this in ArraySet<T>.ReadOnly self, int index)
            => ref self._set._values[index];
    }
}
