using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

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
        public readonly struct ReadOnly : IDisposable, IIsCreated
            , IReadOnlyCollection<ArrayMapKeyValuePair<TKey, TValue>>, IHasCount
            , ITryGetValue<TKey, TValue>
        {
            private static readonly ReadOnly s_empty = new(new ArrayMap<TKey, TValue>());

            internal readonly ArrayMap<TKey, TValue> _map;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly([NotNull] ArrayMap<TKey, TValue> map)
            {
                _map = map;
            }

            public static ReadOnly Empty
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => s_empty;
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(ArrayMap<TKey, TValue> map)
                => map is not null ? map.AsReadOnly() : Empty;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            public ref readonly TValue GetValueByRef(TKey key)
                => ref _map.GetValueByRef(key);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryFindIndex(TKey key, out int index)
                => _map.TryFindIndex(key, out index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int FindIndex(TKey key)
                => _map.FindIndex(key);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<ArrayMapKeyValuePair<TKey, TValue>> IEnumerable<ArrayMapKeyValuePair<TKey, TValue>>.GetEnumerator()
                => _map.GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator()
                => _map.GetEnumerator();
        }
    }
}
