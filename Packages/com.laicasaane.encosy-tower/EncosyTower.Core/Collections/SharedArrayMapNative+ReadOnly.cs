#if !(UNITY_EDITOR || DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections
{
    partial struct SharedArrayMapNative<TKey, TValue>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly()
            => new(this);

        public readonly struct ReadOnly
        {
            internal readonly NativeArray<ArrayMapNode<TKey>>.ReadOnly _valuesInfo;
            internal readonly NativeArray<TValue>.ReadOnly _values;
            internal readonly NativeArray<int>.ReadOnly _buckets;

            internal readonly NativeArray<int>.ReadOnly _freeValueCellIndex;
            internal readonly NativeArray<uint>.ReadOnly _collisions;
            internal readonly NativeArray<ulong>.ReadOnly _fastModBucketsMultiplier;

            public ReadOnly(in SharedArrayMapNative<TKey, TValue> map)
            {
                _valuesInfo = map._valuesInfo.AsReadOnly();
                _values = map._values.AsReadOnly();
                _buckets = map._buckets.AsReadOnly();
                _freeValueCellIndex = map._freeValueCellIndex.AsReadOnly();
                _collisions = map._collisions.AsReadOnly();
                _fastModBucketsMultiplier = map._fastModBucketsMultiplier.AsReadOnly();
            }

            public readonly bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _valuesInfo.IsCreated && _values.IsCreated && _buckets.IsCreated;
            }

            public readonly int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _values.Length;
            }

            public readonly int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _freeValueCellIndex[0];
            }

            public readonly KeyEnumerable Keys
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new(this);
            }

            public readonly ReadOnlySpan<TValue> Values
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _values.AsReadOnlySpan()[.._freeValueCellIndex[0]];
            }

            public TValue this[TKey key]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _values[GetIndex(key)];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(SharedArrayMapNative<TKey, TValue> map)
                => map.AsReadOnly();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly SharedArrayMapNativeReadOnlyKeyValueEnumerator<TKey, TValue> GetEnumerator()
                => new(this);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool ContainsKey(TKey key)
            {
                return TryFindIndex(key, out _);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryGetValue(TKey key, out TValue result)
            {
                if (TryFindIndex(key, out var findIndex))
                {
                    result = _values[findIndex];
                    return true;
                }

                result = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly int GetIndex(TKey key)
            {
                var found = TryFindIndex(key, out var findIndex);

                //Burst is not able to vectorise code if throw is found, regardless if it's actually ever thrown
#if __ENCOSY_VALIDATION__
                if (found == false)
                {
                    ThrowHelper.ThrowKeyNotFoundException_KeyNotFound();
                }
#endif

                return findIndex;
            }

            //I store all the index with an offset + 1, so that in the bucket list 0 means actually not existing.
            //When read the offset must be offset by -1 again to be the real one. In this way
            //I avoid to initialize the array to -1

            //WARNING this method must stay stateless (not relying on states that can change, it's ok to read
            //constant states) because it will be used in multithreaded parallel code
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryFindIndex(TKey key, out int findIndex)
            {
                Checks.IsTrue(_buckets.Length > 0, "Map arrays are not correctly initialized (0 size)");

                var hash = key.GetHashCode();
                var bucketIndex = (int)Reduce((uint)hash, (uint)_buckets.Length, _fastModBucketsMultiplier[0]);
                var valueIndex = _buckets[bucketIndex] - 1;

                //even if we found an existing value we need to be sure it's the one we requested
                while (valueIndex != -1)
                {
                    //Comparer<TKey>.default needs to create a new comparer, so it is much slower
                    //than assuming that Equals is implemented through IEquatable
                    var node = _valuesInfo[valueIndex];

                    if (node._hashcode == hash && node.key.Equals(key))
                    {
                        //this is the one
                        findIndex = valueIndex;
                        return true;
                    }

                    valueIndex = node._previous;
                }

                findIndex = 0;
                return false;
            }

            public readonly struct KeyEnumerable : IEnumerable<TKey>
            {
                private readonly ReadOnly _map;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public KeyEnumerable(in ReadOnly map)
                {
                    _map = map;
                }

                public bool IsValid
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _map.IsCreated;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public KeyEnumerator GetEnumerator()
                    => new(_map);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
                    => GetEnumerator();

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                IEnumerator IEnumerable.GetEnumerator()
                    => GetEnumerator();
            }

            public struct KeyEnumerator : IEnumerator<TKey>
            {
                private readonly ReadOnly _map;
                private readonly int _count;

                private int _index;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public KeyEnumerator(in ReadOnly map) : this()
                {
                    _map = map;
                    _index = -1;
                    _count = map.Count;
                }

                public readonly bool IsValid
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _map.IsCreated;
                }

                public readonly TKey Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _map._valuesInfo.AsReadOnlySpan()[_index].key;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
#if __ENCOSY_VALIDATION__
                    if (IsValid == false)
                    {
                        ThrowHelper.ThrowInvalidOperationException_EnumeratorNotValid();
                    }

                    if (_count != _map.Count)
                    {
                        ThrowHelper.ThrowInvalidOperationException_ModifyWhileBeingIterated_Map();
                    }
#endif

                    if (_index < _count - 1)
                    {
                        ++_index;
                        return true;
                    }

                    return false;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset()
                {
                    _index = -1;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public readonly void Dispose()
                { }

                readonly object IEnumerator.Current => Current;
            }
        }
    }

    public struct SharedArrayMapNativeReadOnlyKeyValueEnumerator<TKey, TValue>
        : IEnumerator<SharedArrayMapNativeReadOnlyKeyValuePairFast<TKey, TValue>>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        private readonly SharedArrayMapNative<TKey, TValue>.ReadOnly _map;

#if __ENCOSY_VALIDATION__
        internal int _startCount;
#endif

        private int _count;
        private int _index;

        public SharedArrayMapNativeReadOnlyKeyValueEnumerator(in SharedArrayMapNative<TKey, TValue>.ReadOnly map) : this()
        {
            _map = map;
            _index = -1;
            _count = map.Count;

#if __ENCOSY_VALIDATION__
            _startCount = map.Count;
#endif
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map.IsCreated;
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

        public readonly SharedArrayMapNativeReadOnlyKeyValuePairFast<TKey, TValue> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_map._valuesInfo.AsReadOnlySpan()[_index].key, _map._values, _index);
        }

        readonly object IEnumerator.Current
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
        public readonly void Dispose() { }
    }

    public readonly struct SharedArrayMapNativeReadOnlyKeyValuePairFast<TKey, TValue>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        private readonly NativeArray<TValue>.ReadOnly _mapValues;
        private readonly TKey _key;
        private readonly int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SharedArrayMapNativeReadOnlyKeyValuePairFast(in TKey key, NativeArray<TValue>.ReadOnly mapValues, int index)
        {
            _mapValues = mapValues;
            _index = index;
            _key = key;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _mapValues.IsCreated;
        }

        public TKey Key
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _key;
        }

        public readonly ref readonly TValue Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _mapValues.AsReadOnlySpan()[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out TKey key, out TValue value)
        {
            key = Key;
            value = Value;
        }
    }
}
