// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/Dictionaries/SveltoDictionary.cs

// MIT License
//
// Copyright (c) 2015-2020 Sebastiano Mandalà
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#if !(UNITY_EDITOR || DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Buffers;
using EncosyTower.Common;
using EncosyTower.Debugging;

namespace EncosyTower.Collections
{
    /// <summary>
    /// Adapts the functionality of <see cref="ArrayMap{TKey, TValue}"/> for a set of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    /// <seealso cref="ArrayMap{TKey, TValue}"/>
    [DebuggerTypeProxy(typeof(ArraySetDebugProxy<>))]
    public partial class ArraySet<T> : IDisposable
        , ICollection<T>, IReadOnlyCollection<T>
        , IClearable, IIncreaseCapacity, IHasCount
        , ICopyToSpan<T>, ITryCopyToSpan<T>
    {
        internal ManagedStrategy<ArrayMapNode<T>> _valuesInfo;
        internal ManagedStrategy<T> _values;
        internal ManagedStrategy<int> _buckets;

        internal int _freeValueCellIndex;
        internal uint _collisions;
        internal ulong _fastModBucketsMultiplier;

        public ArraySet() : this(0) { }

        public ArraySet(int capacity)
        {
            // AllocationStrategy must be passed external for TValue because ArrayMap doesn't have struct
            // constraint needed for the NativeVersion
            _valuesInfo = default;
            _valuesInfo.Alloc(capacity);
            _values = default;
            _values.Alloc(capacity);
            _buckets = default;
            _buckets.Alloc(HashHelpers.GetPrime(capacity));

            if (capacity > 0)
                _fastModBucketsMultiplier = HashHelpers.GetFastModMultiplier((uint)capacity);
        }

        public ArraySet([NotNull] ArraySet<T> source)
        {
            var capacity = source.Capacity;

            _valuesInfo = default;
            _valuesInfo.Alloc(capacity);
            _values = default;
            _values.Alloc(capacity);
            _buckets = default;
            _buckets.Alloc(HashHelpers.GetPrime(capacity));

            source._valuesInfo.AsSpan().CopyTo(_valuesInfo.AsSpan());
            source._values.AsSpan().CopyTo(_values.AsSpan());
            source._buckets.AsSpan().CopyTo(_buckets.AsSpan());

            _freeValueCellIndex = source._freeValueCellIndex;
            _collisions = source._collisions;
            _fastModBucketsMultiplier = source._fastModBucketsMultiplier;
        }

        public ArraySet(ReadOnly source) : this(source._set)
        { }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values.Capacity;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _freeValueCellIndex;
        }

        public ReadOnlyMemory<T> Items
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values.AsArraySegment().Slice(0, _freeValueCellIndex);
        }

        bool ICollection<T>.IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => false;
        }

        public void Dispose()
        {
            _valuesInfo.Dispose();
            _values.Dispose();
            _buckets.Dispose();
        }

        /// <remarks>
        /// This returns readonly because the enumerator cannot be, but at the same time, it cannot be modified
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArraySetEnumerator<T> GetEnumerator()
            => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T value)
        {
            var itemAdded = AddValue(value, out var index);

            if (itemAdded == false)
            {
                return false;
            }
            else
            {
                _values[index] = value;
                return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(in T value)
        {
            var itemAdded = AddValue(value, out var index);

            if (itemAdded == false)
            {
                return false;
            }
            else
            {
                _values[index] = value;
                return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_freeValueCellIndex == 0)
                return;

            _freeValueCellIndex = 0;

            // Buckets cannot be FastCleared because it's important that the values are reset to 0
            _buckets.Clear();

            _values.FastClear();
            _valuesInfo.FastClear();
        }

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
            => new CopyToSpan<T>(Items.Span).CopyTo(sourceStartIndex, destination, length);

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
            => new CopyToSpan<T>(Items.Span).TryCopyTo(sourceStartIndex, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T value)
            => TryFindIndex(value, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
            => _values.AsArraySegment().Slice(0, _freeValueCellIndex).CopyTo(array, arrayIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int size)
        {
            if (_values.Capacity < size)
            {
                var expandPrime = HashHelpers.ExpandPrime(size);

                _values.Resize(expandPrime, true, false);
                _valuesInfo.Resize(expandPrime);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncreaseCapacityBy(int amount)
            => EnsureCapacity(_values.Capacity + amount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncreaseCapacityTo(int size)
            => EnsureCapacity(size);

        public bool Remove(T value)
        {
            var hash = value.GetHashCode();
            var bucketIndex = (int)Reduce((uint)hash, (uint)_buckets.Capacity, _fastModBucketsMultiplier);

            //find the bucket
            var indexToValueToRemove = _buckets[bucketIndex] - 1;
            var itemAfterCurrentOne = -1;
            var comparer = EqualityComparer<T>.Default;

            //Part one: look for the actual key in the bucket list if found I update the bucket list so that it doesn't
            //point anymore to the cell to remove
            while (indexToValueToRemove != -1)
            {
                ref var node = ref _valuesInfo[indexToValueToRemove];
                if (node._hashcode == hash && comparer.Equals(node.key, value))
                {
                    //if the key is found and the bucket points directly to the node to remove
                    if (_buckets[bucketIndex] - 1 == indexToValueToRemove)
                    {
                        //the bucket will point to the previous cell. if a previous cell exists
                        //its next pointer must be updated!
                        //<--- iteration order
                        //                      Bucket points always to the last one
                        //   ------- ------- -------
                        //   |  1  | |  2  | |  3  | //bucket cannot have next, only previous
                        //   ------- ------- -------
                        //--> insert order
                        _buckets[bucketIndex] = node._previous + 1;
                    }
                    else //we need to update the previous pointer if it's not the last element that is removed
                    {
                        Checks.IsTrue(itemAfterCurrentOne != -1, "This should never happen");
                        //update the previous pointer of the item after the one to remove with the previous pointer of the item to remove
                        _valuesInfo[itemAfterCurrentOne]._previous = node._previous;
                    }

                    break; //don't miss this, at this point it must break and not update indexToValueToRemove
                }

                //a bucket always points to the last element of the list, so if the item is not found we need to iterate backward
                itemAfterCurrentOne = indexToValueToRemove;
                indexToValueToRemove = node._previous;
            }

            if (indexToValueToRemove == -1)
            {
                return false; //not found!
            }

            _freeValueCellIndex--; //one less value to iterate

            //Part two:
            //At this point nodes pointers and buckets are updated, but the _values array
            //still has got the value to delete. Remember the goal of this map is to be able
            //to iterate over the values like an array, so the values array must always be up to date

            //if the cell to remove is the last one in the list, we can perform less operations (no swapping needed)
            //otherwise we want to move the last value cell over the value to remove

            var lastValueCellIndex = _freeValueCellIndex;
            if (indexToValueToRemove != lastValueCellIndex)
            {
                //we can transfer the last value of both arrays to the index of the value to remove.
                //in order to do so, we need to be sure that the bucket pointer is updated.
                //first we find the index in the bucket list of the pointer that points to the cell
                //to move
                ref var modeToMove = ref _valuesInfo[lastValueCellIndex];

                var movingBucketIndex = (int)Reduce(
                      (uint)modeToMove._hashcode
                    , (uint)_buckets.Capacity
                    , _fastModBucketsMultiplier
                );

                var linkedListIterationIndex = _buckets[movingBucketIndex] - 1;

                //if the key is found and the bucket points directly to the node to remove
                //it must now point to the cell where it's going to be moved (update bucket list first linked list node to iterate from)
                if (linkedListIterationIndex == lastValueCellIndex)
                    _buckets[movingBucketIndex] = indexToValueToRemove + 1;

                //find the prev element of the last element in the valuesInfo array
                while (_valuesInfo[linkedListIterationIndex]._previous != -1 && _valuesInfo[linkedListIterationIndex]._previous != lastValueCellIndex)
                    linkedListIterationIndex = _valuesInfo[linkedListIterationIndex]._previous;

                //if we find any value that has the last value cell as previous, we need to update it to point to the new value index that is going to be replaced
                if (_valuesInfo[linkedListIterationIndex]._previous != -1)
                    _valuesInfo[linkedListIterationIndex]._previous = indexToValueToRemove;

                //finally, actually move the values
                _valuesInfo[indexToValueToRemove] = modeToMove;
                _values[indexToValueToRemove] = _values[lastValueCellIndex];
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            _values.Resize(_freeValueCellIndex);
            _valuesInfo.Resize(_freeValueCellIndex);
        }

        //I store all the index with an offset + 1, so that in the bucket list 0 means actually not existing.
        //When read the offset must be offset by -1 again to be the real one. In this way
        //I avoid to initialize the array to -1

        //WARNING this method must stay stateless (not relying on states that can change, it's ok to read
        //constant states) because it will be used in multithreaded parallel code
        private bool TryFindIndex(T value, out int index)
        {
            Checks.IsTrue(_buckets.Capacity > 0, "Set arrays are not correctly initialized (0 size)");

            var hash = value.GetHashCode();
            var bucketIndex = (int)Reduce((uint)hash, (uint)_buckets.Capacity, _fastModBucketsMultiplier);
            var valueIndex = _buckets[bucketIndex] - 1;
            var comparer = EqualityComparer<T>.Default;

            //even if we found an existing value we need to be sure it's the one we requested
            while (valueIndex != -1)
            {
                ref var node = ref _valuesInfo[valueIndex];
                if (node._hashcode == hash && comparer.Equals(node.key, value))
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Intersect([NotNull] ArraySet<T> otherMapKeys)
        {
            var items = Items.Span;

            for (var i = Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (otherMapKeys.Contains(item) == false) Remove(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exclude([NotNull] ArraySet<T> otherMapKeys)
        {
            var items = Items.Span;

            for (var i = Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (otherMapKeys.Contains(item)) Remove(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Union([NotNull] ArraySet<T> otherMapKeys)
        {
            foreach (var other in otherMapKeys)
            {
                Add(other);
            }
        }

        private bool AddValue(T value, out int indexSet)
        {
            var hash = value.GetHashCode(); //IEquatable doesn't enforce the override of GetHashCode
            var bucketIndex = (int)Reduce((uint)hash, (uint)_buckets.Capacity, _fastModBucketsMultiplier);

            //buckets value -1 means it's empty
            var valueIndex = _buckets[bucketIndex] - 1;

            if (valueIndex == -1)
            {
                ResizeIfNeeded();
                //create the info node at the last position and fill it with the relevant information
                _valuesInfo[_freeValueCellIndex] = new ArrayMapNode<T>(value, hash);
            }
            else //collision or already exists
            {
                var currentValueIndex = valueIndex;
                var comparer = EqualityComparer<T>.Default;

                do
                {
                    //must check if the key already exists in the map
                    ref var node = ref _valuesInfo[currentValueIndex];
                    if (node._hashcode == hash && comparer.Equals(node.key, value))
                    {
                        //the key already exists, simply replace the value!
                        indexSet = currentValueIndex;
                        return false;
                    }

                    currentValueIndex = node._previous;
                } while (currentValueIndex != -1); //-1 means no more values with key with the same hash

                ResizeIfNeeded();

                //oops collision!
                _collisions++;
                //create a new node which previous index points to node currently pointed in the bucket (valueIndex)
                //_freeValueCellIndex = valueIndex + 1
                _valuesInfo[_freeValueCellIndex] = new ArrayMapNode<T>(value, hash, valueIndex);
                //Important: the new node is always the one that will be pointed by the bucket cell
                //so I can assume that the one pointed by the bucket is always the last value added
            }

            //item with this bucketIndex will point to the last value created
            //ToDo: if instead I assume that the original one is the one in the bucket
            //I wouldn't need to update the bucket here. Small optimization but important
            _buckets[bucketIndex] = _freeValueCellIndex + 1;

            indexSet = _freeValueCellIndex;
            _freeValueCellIndex++;

            //too many collisions
            if (_collisions > _buckets.Capacity)
            {
                if (_buckets.Capacity < 100)
                    RecomputeBuckets((int)_collisions << 1);
                else
                    RecomputeBuckets(HashHelpers.ExpandPrime((int)_collisions));
            }

            return true;
        }

        private void RecomputeBuckets(int newSize)
        {
            //we need more space and less collisions
            _buckets.Resize(newSize, false);
            _collisions = 0;
            _fastModBucketsMultiplier = HashHelpers.GetFastModMultiplier((uint)_buckets.Capacity);
            var bucketsCapacity = (uint)_buckets.Capacity;

            //we need to get all the hash code of all the values stored so far and spread them over the new bucket
            //length
            var freeValueCellIndex = _freeValueCellIndex;
            for (var newValueIndex = 0; newValueIndex < freeValueCellIndex; ++newValueIndex)
            {
                //get the original hash code and find the new bucketIndex due to the new length
                ref var valueInfoNode = ref _valuesInfo[newValueIndex];
                var bucketIndex = (int)Reduce((uint)valueInfoNode._hashcode, bucketsCapacity, _fastModBucketsMultiplier);
                //bucketsIndex can be -1 or a next value. If it's -1 means no collisions. If there is collision,
                //we create a new node which prev points to the old one. Old one next points to the new one.
                //the bucket will now points to the new one
                //In this way we can rebuild the linkedlist.
                //get the current valueIndex, it's -1 if no collision happens
                var existingValueIndex = _buckets[bucketIndex] - 1;
                //update the bucket index to the index of the current item that share the bucketIndex
                //(last found is always the one in the bucket)
                _buckets[bucketIndex] = newValueIndex + 1;
                if (existingValueIndex == -1)
                {
                    //ok nothing was indexed, the bucket was empty. We need to update the previous
                    //values of next and previous
                    valueInfoNode._previous = -1;
                }
                else
                {
                    //oops a value was already being pointed by this cell in the new bucket list,
                    //it means there is a collision, problem
                    _collisions++;
                    //the bucket will point to this value, so
                    //the previous index will be used as previous for the new value.
                    valueInfoNode._previous = existingValueIndex;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeIfNeeded()
        {
            if (_freeValueCellIndex != _values.Capacity)
            {
                return;
            }

            var expandPrime = HashHelpers.ExpandPrime(_freeValueCellIndex);

            _values.Resize(expandPrime, true, false);
            _valuesInfo.Resize(expandPrime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Reduce(uint hashcode, uint N, ulong fastModBucketsMultiplier)
        {
            if (hashcode >= N) //is the condition return actually an optimization?
                return Environment.Is64BitProcess
                    ? HashHelpers.FastMod(hashcode, N, fastModBucketsMultiplier)
                    : hashcode % N;

            return hashcode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICollection<T>.Add(T item)
            => Add(item);
    }

    public struct ArraySetEnumerator<T> : IEnumerator<T>
    {
        private readonly ArraySet<T> _set;

#if __ENCOSY_VALIDATION__
        internal int _startCount;
#endif

        private int _count;
        private int _index;

        public ArraySetEnumerator([NotNull] ArraySet<T> set) : this()
        {
            _set = set;
            _index = -1;
            _count = set.Count;

#if __ENCOSY_VALIDATION__
            _startCount = set.Count;
#endif
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _set != null;
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
                ThrowHelper.ThrowInvalidOperationException_ModifyWhileBeingIterated_Set();
            }
#endif

            if (_index >= _count - 1)
            {
                return false;
            }

            ++_index;
            return true;
        }

        public readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _set._values[_index];
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
            _count = _set.Count;

#if __ENCOSY_VALIDATION__
            _startCount = _set.Count;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() { }
    }

    internal sealed class ArraySetDebugProxy<T>
    {

        private readonly ArraySet<T> _set;

        public ArraySetDebugProxy([NotNull] ArraySet<T> set)
        {
            _set = set;
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (uint)_set.Count;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Values
        {
            get
            {
                var set = _set;
                var array = new T[set.Count];
                var i = 0;

                foreach (var value in set)
                {
                    array[i++] = value;
                }

                return array;
            }
        }
    }
}
