#if UNITY_COLLECTIONS
#if !(UNITY_EDITOR || DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Buffers;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections
{
    partial struct ArraySetNative<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnly AsReadOnly()
            => new(this);

        public readonly struct ReadOnly : IIsCreated, IHasCount, IHasCapacity
        {
            internal readonly NativeStrategy<ArrayMapNode<T>>.ReadOnly _valuesInfo;
            internal readonly NativeStrategy<T>.ReadOnly _values;
            internal readonly NativeStrategy<int>.ReadOnly _buckets;

            internal readonly NativeReference<ulong>.ReadOnly _fastModBucketsMultiplier;
            internal readonly NativeReference<uint>.ReadOnly _collisions;
            internal readonly NativeReference<int>.ReadOnly _freeValueCellIndex;
            internal readonly NativeReference<int>.ReadOnly _version;

            public ReadOnly(ArraySetNative<T> set)
            {
                _valuesInfo = set._valuesInfo.AsReadOnly();
                _values = set._values.AsReadOnly();
                _buckets = set._buckets.AsReadOnly();
                _freeValueCellIndex = set._freeValueCellIndex.AsReadOnly();
                _version = set._version.AsReadOnly();
                _collisions = set._collisions.AsReadOnly();
                _fastModBucketsMultiplier = set._fastModBucketsMultiplier.AsReadOnly();
            }

            public readonly bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _valuesInfo.IsCreated && _values.IsCreated && _buckets.IsCreated;
            }

            public readonly int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _values.Capacity;
            }

            public readonly int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _freeValueCellIndex.Value;
            }

            public readonly NativeSliceReadOnly<T> Items
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _values.AsNativeSliceReadOnly().Slice(0, _freeValueCellIndex.Value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly ArraySetNativeReadOnlyEnumerator<T> GetEnumerator()
                => new(this);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<T> destination)
                => CopyTo(0, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<T> destination, int length)
                => CopyTo(0, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, Span<T> destination)
                => CopyTo(sourceStartIndex, destination, destination.Length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, Span<T> destination, int length)
                => new CopyToSpan<T>(Items.AsSpan()).CopyTo(sourceStartIndex, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(Span<T> destination)
                => TryCopyTo(0, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(Span<T> destination, int length)
                => TryCopyTo(0, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, Span<T> destination)
                => TryCopyTo(sourceStartIndex, destination, destination.Length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
                => new CopyToSpan<T>(Items.AsSpan()).TryCopyTo(sourceStartIndex, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Contains(T value)
                => TryFindIndex(value, out _);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Contains(in T value)
                => TryFindIndex(value, out _);

            //I store all the index with an offset + 1, so that in the bucket list 0 means actually not existing.
            //When read the offset must be offset by -1 again to be the real one. In this way
            //I avoid to initialize the array to -1

            //WARNING this method must stay stateless (not relying on states that can change, it's ok to read
            //constant states) because it will be used in multithreaded parallel code
            private readonly bool TryFindIndex(in T value, out int index)
            {
                Checks.IsTrue(_buckets.Capacity > 0, "Map arrays are not correctly initialized (0 size)");

                var hash = value.GetHashCode();
                var bucketIndex = (int)Reduce((uint)hash, (uint)_buckets.Capacity, _fastModBucketsMultiplier.Value);
                var valueIndex = _buckets[bucketIndex] - 1;

                //even if we found an existing value we need to be sure it's the one we requested
                while (valueIndex != -1)
                {
                    //Comparer<T>.default needs to create a new comparer, so it is much slower
                    //than assuming that Equals is implemented through IEquatable
                    ref readonly var node = ref _valuesInfo[valueIndex];
                    if (node._hashcode == hash && node.key.Equals(value))
                    {
                        //this is the one
                        index = valueIndex;
                        return true;
                    }

                    valueIndex = node._previous;
                }

                index = 0;
                return false;
            }
        }
    }

    public struct ArraySetNativeReadOnlyEnumerator<T> : IEnumerator<T>
        where T : unmanaged, IEquatable<T>
    {
        private readonly ArraySetNative<T>.ReadOnly _set;

#if __ENCOSY_VALIDATION__
        private readonly int _version;
#endif

        private int _index;

        public ArraySetNativeReadOnlyEnumerator(in ArraySetNative<T>.ReadOnly set) : this()
        {
            _set = set;
            _index = -1;

#if __ENCOSY_VALIDATION__
            _version = set._version.Value;
#endif
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _set.IsCreated;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
#if __ENCOSY_VALIDATION__
            if (IsValid == false)
            {
                ThrowHelper.ThrowInvalidOperationException_EnumeratorNotValid();
            }

            if (_version != _set._version.Value)
            {
                ThrowHelper.ThrowInvalidOperationException_ModifyWhileBeingIterated_Set();
            }
#endif

            if (_index < _set.Count - 1)
            {
                ++_index;
                return true;
            }

            return false;
        }

        public readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _set._valuesInfo[_index].key;
        }

        readonly object IEnumerator.Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Current;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() { }
    }
}

#endif
