#if UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Buffers;

namespace EncosyTower.Collections.Extensions.Unsafe
{
    public static class ArrayMapNativeExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeStrategy<ArrayMapNode<TKey>> GetKeysUnsafe<TKey, TValue>(this in ArrayMapNative<TKey, TValue> self)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            => self._valuesInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeStrategy<TValue> GetValuesUnsafe<TKey, TValue>(this in ArrayMapNative<TKey, TValue> self)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            => self._values;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NB<TValue> GetValuesUnsafe<TKey, TValue>(this in ArrayMapNative<TKey, TValue> self, out int count)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            count = self._freeValueCellIndex.Value;
            return self._values.ToRealBuffer();
        }
    }
}

#endif
