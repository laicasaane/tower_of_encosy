#if !(UNITY_EDITOR || DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Debugging;

namespace EncosyTower.Serialization.Collections
{
    public class JsonArrayMap<TKey, TValue>
        : ArrayMap<TKey, TValue>, IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private KeyCollection _keyCol;
        private ValueCollection _valueCol;

        public JsonArrayMap() : base() { }

        public JsonArrayMap(int capacity) : base(capacity) { }

        public JsonArrayMap([NotNull] ArrayMap<TKey, TValue> source) : base(source) { }

        public JsonArrayMap(ReadOnly source) : base(source) { }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => _keyCol ??= new KeyCollection(this);

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _keyCol ??= new KeyCollection(this);

        ICollection<TValue> IDictionary<TKey, TValue>.Values => _valueCol ??= new ValueCollection(this);

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _valueCol ??= new ValueCollection(this);

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            Add(key, value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw ThrowHelper.CreateArgumentNullException(nameof(array));
            }

            if ((uint)arrayIndex > (uint)array.Length)
            {
                throw ThrowHelper.CreateArgumentOutOfRangeException_IndexNegative();
            }

            if (array.Length - arrayIndex < Count)
            {
                throw ThrowHelper.CreateArgumentException_ArrayPlusOffTooSmall();
            }

            foreach (var (key, value) in this)
            {
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(key, value);
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new MapEnumerator(this);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return TryGetValue(item.Key, out var value)
                && EqualityComparer<TValue>.Default.Equals(value, item.Value)
                && Remove(item.Key);
        }

        private sealed class KeyCollection : ICollection<TKey>
        {
            private readonly ArrayMap<TKey, TValue> _map;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyCollection([NotNull] ArrayMap<TKey, TValue> map)
            {
                _map = map;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map.Count;
            }

            public bool IsReadOnly => true;

            void ICollection<TKey>.Add(TKey item)
            {
                throw new NotSupportedException("Collection is read-only.");
            }

            void ICollection<TKey>.Clear()
            {
                throw new NotSupportedException("Collection is read-only.");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(TKey item)
            {
                return _map.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw ThrowHelper.CreateArgumentNullException(nameof(array));
                }

                if ((uint)arrayIndex > (uint)array.Length)
                {
                    throw ThrowHelper.CreateArgumentOutOfRangeException_IndexNegative();
                }

                if (array.Length - arrayIndex < _map.Count)
                {
                    throw ThrowHelper.CreateArgumentException_ArrayPlusOffTooSmall();
                }

                foreach (var key in _map.Keys)
                {
                    array[arrayIndex++] = key;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyEnumerator GetEnumerator()
            {
                return new KeyEnumerator(_map);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return GetEnumerator();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            bool ICollection<TKey>.Remove(TKey item)
            {
                throw new NotSupportedException("Collection is read-only.");
            }
        }

        private sealed class ValueCollection : ICollection<TValue>
        {
            private readonly ArrayMap<TKey, TValue> _map;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueCollection([NotNull] ArrayMap<TKey, TValue> map)
            {
                _map = map;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map.Count;
            }

            public bool IsReadOnly => true;

            void ICollection<TValue>.Add(TValue item)
            {
                throw new NotSupportedException("Collection is read-only.");
            }

            void ICollection<TValue>.Clear()
            {
                throw new NotSupportedException("Collection is read-only.");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(TValue item)
            {
                var items = _map.Values.Span;
                var length = items.Length;
                var comparer = EqualityComparer<TValue>.Default;

                for (var i = 0; i < length; i++)
                {
                    if (comparer.Equals(items[i], item))
                    {
                        return true;
                    }
                }

                return false;
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw ThrowHelper.CreateArgumentNullException(nameof(array));
                }

                if ((uint)arrayIndex > (uint)array.Length)
                {
                    throw ThrowHelper.CreateArgumentOutOfRangeException_IndexNegative();
                }

                if (array.Length - arrayIndex < _map.Count)
                {
                    throw ThrowHelper.CreateArgumentException_ArrayPlusOffTooSmall();
                }

                foreach (var value in _map.Values.Span)
                {
                    array[arrayIndex++] = value;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueEnumerator GetEnumerator()
            {
                return new ValueEnumerator(_map);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return GetEnumerator();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            bool ICollection<TValue>.Remove(TValue item)
            {
                throw new NotSupportedException("Collection is read-only.");
            }
        }

        private sealed new class KeyEnumerator : IEnumerator<TKey>
        {
            private ArrayMap<TKey, TValue>.KeyEnumerator _enumerator;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyEnumerator([NotNull] ArrayMap<TKey, TValue> map)
            {
                _enumerator = new(map);
            }

            public TKey Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _enumerator.Current;
            }

            object IEnumerator.Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Current;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _enumerator.Reset();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }
        }

        private sealed class ValueEnumerator : IEnumerator<TValue>
        {
            private ArrayMapKeyValueEnumerator<TKey, TValue> _enumerator;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueEnumerator([NotNull] ArrayMap<TKey, TValue> map)
            {
                _enumerator = map.GetEnumerator();
            }

            public TValue Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _enumerator.Current.Value;
            }

            object IEnumerator.Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Current;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _enumerator.Reset();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _enumerator = default;
            }
        }

        private sealed class MapEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private ArrayMap<TKey, TValue> _map;

#if __ENCOSY_VALIDATION__
            internal int _startCount;
#endif

            private int _count;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public MapEnumerator([NotNull] ArrayMap<TKey, TValue> map)
            {
                _map = map;
                _index = -1;
                _count = map.Count;

#if __ENCOSY_VALIDATION__
                _startCount = map.Count;
#endif
            }

            public bool IsValid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map != null;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
#if __ENCOSY_VALIDATION__
                if (IsValid == false)
                {
                    ThrowHelper.ThrowInvalidOperationException_EnumeratorNotValid();
                }

                if (_count != _startCount)
                {
                    ThrowHelper.ThrowInvalidOperationException_ModifyWhileBeingIterated_Map();
                }
#endif

                if (_index >= _count - 1)
                {
                    return false;
                }

                ++_index;
                return true;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new(_map._valuesInfo[_index].key, _map._values[_index]);
            }

            object IEnumerator.Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Current;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetRange(uint startIndex, uint count)
            {
                _index = (int)startIndex - 1;
                _count = (int)count;

#if __ENCOSY_VALIDATION__
                if (IsValid == false)
                {
                    ThrowHelper.ThrowInvalidOperationException_EnumeratorNotValid();
                }

                if (_count > _startCount)
                {
                    ThrowHelper.ThrowInvalidOperationException_SetCountGreaterThanStartingOne();
                }

                _startCount = (int)count;
#endif
            }

            public void Reset()
            {
                _index = -1;
                _count = _map.Count;

#if __ENCOSY_VALIDATION__
                _startCount = _map.Count;
#endif
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _map = null;
            }
        }
    }
}
