using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections
{
    partial class ArrayMap<TKey, TValue>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly()
        {
            return new(this);
        }

        /// <inheritdoc cref="ArrayMap{TKey, TValue}"/>
        [DebuggerTypeProxy(typeof(ArrayMapDebugProxy<,>))]
        public readonly struct ReadOnly : IDisposable
            , IReadOnlyCollection<ArrayMapKeyValuePairFast<TKey, TValue>>
        {
            internal readonly ArrayMap<TKey, TValue> _map;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly([NotNull] ArrayMap<TKey, TValue> map)
            {
                _map = map;
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map != null;
            }

            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map.Capacity;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map._freeValueCellIndex;
            }

            public KeyEnumerable Keys
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new(_map);
            }

            public ArraySegment<TValue> Values
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map.Values;
            }

            public TValue this[TKey key]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map[key];
            }

            public void Dispose()
            {
                _map.Dispose();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ArrayMapKeyValueEnumerator<TKey, TValue> GetEnumerator()
                => _map.GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ContainsKey(TKey key)
                => _map.ContainsKey(key);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue(TKey key, out TValue result)
                => _map.TryGetValue(key, out result);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref TValue GetValueByRef(TKey key)
                => ref _map.GetValueByRef(key);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryFindIndex(TKey key, out int findIndex)
                => _map.TryFindIndex(key, out findIndex);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetIndex(TKey key)
                => _map.GetIndex(key);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<ArrayMapKeyValuePairFast<TKey, TValue>> IEnumerable<ArrayMapKeyValuePairFast<TKey, TValue>>.GetEnumerator()
                => _map.GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator()
                => _map.GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(ArrayMap<TKey, TValue> map)
            {
                return map.AsReadOnly();
            }
        }
    }
}
