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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TValue GetValueAtUnsafe<TKey, TValue>(this in ArrayMapNative<TKey, TValue> self, int index)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            => ref self._values[index];
    }

    public static class ArrayMapNativeReadOnlyExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeStrategy<ArrayMapNode<TKey>>.ReadOnly GetKeysUnsafe<TKey, TValue>(this in ArrayMapNative<TKey, TValue>.ReadOnly self)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            => self._valuesInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeStrategy<TValue>.ReadOnly GetValuesUnsafe<TKey, TValue>(this in ArrayMapNative<TKey, TValue>.ReadOnly self)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            => self._values;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NB<TValue>.ReadOnly GetValuesUnsafe<TKey, TValue>(this in ArrayMapNative<TKey, TValue>.ReadOnly self, out int count)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            count = self._freeValueCellIndex.Value;
            return self._values.ToRealBuffer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly TValue GetValueAtUnsafe<TKey, TValue>(this in ArrayMapNative<TKey, TValue>.ReadOnly self, int index)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            => ref self._values[index];
    }
}

#endif
