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

// ReSharper disable InconsistentNaming

#if DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG
#define ENABLE_DEBUG_CHECKS
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Modules.Buffers;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Modules.Collections
{
    /// <summary>
    /// This map has been created for just one reason: I needed a map that would have let me iterate
    /// over the values as an array, directly, without generating one or using an iterator.
    /// For this goal is N times faster than the standard Dictionary. This map is also faster than
    /// the standard Dictionary for most of the operations, but the difference is negligible. The only slower operation
    /// is resizing the memory on add, as this implementation needs to use two separate arrays compared to the standard
    /// one
    /// note: ArrayMap is not thread safe. A thread safe version should take care of possible setting of
    /// value with shared hash hence bucket list index.
    /// </summary>
    [DebuggerTypeProxy(typeof(ArrayMapDebugProxy<,>))]
    public class ArrayMap<TKey, TValue> : IDisposable
        , ICollection<KeyValuePairFast<TKey, TValue>>
        , IReadOnlyCollection<KeyValuePairFast<TKey, TValue>>
    {
        internal readonly static EqualityComparer<TKey> s_comparer = EqualityComparer<TKey>.Default;

        internal ManagedStrategy<ArrayMapNode<TKey>> _valuesInfo;
        internal ManagedStrategy<TValue> _values;
        internal ManagedStrategy<int> _buckets;

        internal int _freeValueCellIndex;
        internal uint _collisions;
        internal ulong _fastModBucketsMultiplier;

        public ArrayMap() : this(0) { }

        public ArrayMap(int capacity)
        {
            // AllocationStrategy must be passed external for TValue because ArrayMap doesn't have struct
            // constraint needed for the NativeVersion
            _valuesInfo = default;
            _valuesInfo.Alloc(capacity, default);
            _values = default;
            _values.Alloc(capacity, default);
            _buckets = default;
            _buckets.Alloc(HashHelpers.GetPrime(capacity), default);

            if (capacity > 0)
                _fastModBucketsMultiplier = HashHelpers.GetFastModMultiplier((uint)capacity);
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _freeValueCellIndex;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buckets.IsValid;
        }

        public KeyEnumerable Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(this);
        }

        bool ICollection<KeyValuePairFast<TKey, TValue>>.IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => false;
        }

        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[GetIndex(key)];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                AddValue(key, out var index);

                _values[index] = value;
            }
        }

        /// <remarks>
        /// This returns readonly because the enumerator cannot be, but at the same time, it cannot be modified
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArrayMapKeyValueEnumerator<TKey, TValue> GetEnumerator()
            => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<KeyValuePairFast<TKey, TValue>> IEnumerable<KeyValuePairFast<TKey, TValue>>.GetEnumerator()
            => new ArrayMapKeyValueEnumerator<TKey, TValue>(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => new ArrayMapKeyValueEnumerator<TKey, TValue>(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, in TValue value)
        {
            var itemAdded = AddValue(key, out var index);

#if ENABLE_DEBUG_CHECKS
            if (itemAdded == false)
                throw new InvalidOperationException("Key already present");
#endif

            _values[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, in TValue value)
        {
            var itemAdded = AddValue(key, out var index);

            if (itemAdded)
                _values[index] = value;

            return itemAdded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, in TValue value, out int index)
        {
            var itemAdded = AddValue(key, out index);

            if (itemAdded)
                _values[index] = value;

            return itemAdded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(TKey key, in TValue value)
        {
            var itemAdded = AddValue(key, out var index);

#if ENABLE_DEBUG_CHECKS
            if (itemAdded)
                throw new InvalidOperationException("Trying to set a value on a not existing key");
#endif

            _values[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Recycle()
        {
            if (_freeValueCellIndex == 0)
                return;

            _freeValueCellIndex = 0;

            // Buckets cannot be FastCleared because it's important that the values are reset to 0
            _buckets.Clear();
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
        public bool ContainsKey(TKey key)
        {
            return TryFindIndex(key, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue result)
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
        public ref TValue GetOrAdd(TKey key)
        {
            if (TryFindIndex(key, out var findIndex))
            {
                return ref _values[findIndex];
            }

            AddValue(key, out findIndex);

            _values[findIndex] = default;

            return ref _values[findIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrAdd(TKey key, Func<TValue> builder)
        {
            if (TryFindIndex(key, out var findIndex))
            {
                return ref _values[findIndex];
            }

            AddValue(key, out findIndex);

            _values[findIndex] = builder();

            return ref _values[findIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrAdd(TKey key, out int index)
        {
            if (TryFindIndex(key, out index))
            {
                return ref _values[index];
            }

            AddValue(key, out index);

            return ref _values[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrAdd<W>(TKey key, FuncRef<W, TValue> builder, ref W parameter)
        {
            if (TryFindIndex(key, out var findIndex))
            {
                return ref _values[findIndex];
            }

            AddValue(key, out findIndex);

            _values[findIndex] = builder(ref parameter);

            return ref _values[findIndex];
        }

        /// <summary>
        /// RecycledOrCreate makes sense to use on maps that are fast cleared and use objects
        /// as value. Once the map is fast cleared, it will try to reuse object values that are
        /// recycled during the fast clearing.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="builder"></param>
        /// <param name="recycler"></param>
        /// <typeparam name="TValueProxy"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue RecycleOrAdd<TValueProxy>(
              TKey key
            , Func<TValueProxy> builder
            , ActionRef<TValueProxy> recycler
        )
            where TValueProxy : class, TValue
        {
            if (TryFindIndex(key, out var findIndex))
            {
                return ref _values[findIndex];
            }

            AddValue(key, out findIndex);

            if (_values[findIndex] == null)
                _values[findIndex] = builder();
            else
                recycler(ref UnsafeUtility.As<TValue, TValueProxy>(ref _values[findIndex]));

            return ref _values[findIndex];
        }

        /// <summary>
        /// RecycledOrCreate makes sense to use on maps that are fast cleared and use objects
        /// as value. Once the map is fast cleared, it will try to reuse object values that are
        /// recycled during the fast clearing.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="builder"></param>
        /// <param name="recycler"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="TValueProxy"></typeparam>
        /// <typeparam name="W"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue RecycleOrAdd<TValueProxy, W>(
              TKey key
            , FuncRef<W, TValue> builder
            , ActionRef<TValueProxy, W> recycler, ref W parameter
        )
            where TValueProxy : class, TValue
        {
            if (TryFindIndex(key, out var findIndex))
            {
                return ref _values[findIndex];
            }

            AddValue(key, out findIndex);

            if (_values[findIndex] == null)
                _values[findIndex] = builder(ref parameter);
            else
                recycler(ref UnsafeUtility.As<TValue, TValueProxy>(ref _values[findIndex]), ref parameter);

            return ref _values[findIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueByRef(TKey key)
        {
#if ENABLE_DEBUG_CHECKS
            if (TryFindIndex(key, out var findIndex))
                return ref _values[findIndex];

            throw new KeyNotFoundException("Key not found");
#else
            // Burst is not able to vectorise code if throw is found, regardless if it's actually ever thrown
            TryFindIndex(key, out var findIndex);

            return ref _values[(int)findIndex];
#endif
        }

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
        public void IncreaseCapacityBy(int size)
        {
            var expandPrime = HashHelpers.ExpandPrime(_values.Capacity + size);

            _values.Resize(expandPrime, true, false);
            _valuesInfo.Resize(expandPrime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
            return Remove(key, out _, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key, out int index, out TValue value)
        {
            var hash = key.GetHashCode();
            var bucketIndex = (int)Reduce((uint)hash, (uint)_buckets.Capacity, _fastModBucketsMultiplier);

            //find the bucket
            var indexToValueToRemove = _buckets[bucketIndex] - 1;
            var itemAfterCurrentOne = -1;
            var comparer = s_comparer;

            //Part one: look for the actual key in the bucket list if found I update the bucket list so that it doesn't
            //point anymore to the cell to remove
            while (indexToValueToRemove != -1)
            {
                ref var node = ref _valuesInfo[indexToValueToRemove];
                if (node._hashcode == hash && comparer.Equals(node.key, key))
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
                index = default;
                value = default;
                return false; //not found!
            }

            index = indexToValueToRemove; //index is a out variable, for internal use we want to know the index of the element to remove

            _freeValueCellIndex--; //one less value to iterate
            value = _values[indexToValueToRemove]; //value is a out variable, we want to know the value of the element to remove

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindIndex(TKey key, out int findIndex)
        {
            Checks.IsTrue(_buckets.Capacity > 0, "Map arrays are not correctly initialized (0 size)");

            var hash = key.GetHashCode();
            var bucketIndex = (int)Reduce((uint)hash, (uint)_buckets.Capacity, _fastModBucketsMultiplier);
            var valueIndex = _buckets[bucketIndex] - 1;
            var comparer = s_comparer;

            //even if we found an existing value we need to be sure it's the one we requested
            while (valueIndex != -1)
            {
                ref var node = ref _valuesInfo[valueIndex];
                if (node._hashcode == hash && comparer.Equals(node.key, key))
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(TKey key)
        {
#if ENABLE_DEBUG_CHECKS
            if (TryFindIndex(key, out var findIndex))
                return findIndex;

            throw new KeyNotFoundException("Key not found");
#else
            //Burst is not able to vectorise code if throw is found, regardless if it's actually ever thrown
            TryFindIndex(key, out var findIndex);

            return findIndex;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Intersect<UValue>(in ArrayMap<TKey, UValue> otherMapKeys)
        {
            var keys = _valuesInfo.AsSpan();

            for (var i = Count - 1; i >= 0; i--)
            {
                var tKey = keys[i].key;
                if (otherMapKeys.ContainsKey(tKey) == false) Remove(tKey);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exclude<UValue>(in ArrayMap<TKey, UValue> otherMapKeys)
        {
            var keys = _valuesInfo.AsSpan();

            for (var i = Count - 1; i >= 0; i--)
            {
                var tKey = keys[i].key;
                if (otherMapKeys.ContainsKey(tKey)) Remove(tKey);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Union(in ArrayMap<TKey, TValue> otherMapKeys)
        {
            foreach (var other in otherMapKeys)
            {
                this[other.Key] = other.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AddValue(TKey key, out int indexSet)
        {
            var hash = key.GetHashCode(); //IEquatable doesn't enforce the override of GetHashCode
            var bucketIndex = (int)Reduce((uint)hash, (uint)_buckets.Capacity, _fastModBucketsMultiplier);

            //buckets value -1 means it's empty
            var valueIndex = _buckets[bucketIndex] - 1;

            if (valueIndex == -1)
            {
                ResizeIfNeeded();
                //create the info node at the last position and fill it with the relevant information
                _valuesInfo[_freeValueCellIndex] = new ArrayMapNode<TKey>(key, hash);
            }
            else //collision or already exists
            {
                var currentValueIndex = valueIndex;
                var comparer = s_comparer;

                do
                {
                    //must check if the key already exists in the map
                    ref var node = ref _valuesInfo[currentValueIndex];
                    if (node._hashcode == hash && comparer.Equals(node.key, key))
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
                _valuesInfo[_freeValueCellIndex] = new ArrayMapNode<TKey>(key, hash, valueIndex);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        public readonly struct KeyEnumerable
        {
            private readonly ArrayMap<TKey, TValue> _map;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyEnumerable(in ArrayMap<TKey, TValue> map)
            {
                _map = map;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyEnumerator GetEnumerator()
                => new(_map);
        }

        public struct KeyEnumerator
        {
            private readonly ArrayMap<TKey, TValue> _map;
            private readonly int _count;

            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyEnumerator(in ArrayMap<TKey, TValue> map) : this()
            {
                _map = map;
                _index = -1;
                _count = map.Count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
#if ENABLE_DEBUG_CHECKS
                if (_count != _map.Count)
                    throw new InvalidOperationException("Cannot modify a map while it is being iterated");
#endif
                if (_index < _count - 1)
                {
                    ++_index;
                    return true;
                }

                return false;
            }

            public TKey Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map._valuesInfo[_index].key;
            }
        }

        public void Dispose()
        {
            _valuesInfo.Dispose();
            _values.Dispose();
            _buckets.Dispose();
        }

        void ICollection<KeyValuePairFast<TKey, TValue>>.Add(KeyValuePairFast<TKey, TValue> item)
        {
            TryAdd(item.Key, item.Value);
        }

        bool ICollection<KeyValuePairFast<TKey, TValue>>.Contains(KeyValuePairFast<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        void ICollection<KeyValuePairFast<TKey, TValue>>.CopyTo(KeyValuePairFast<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePairFast<TKey, TValue>>.Remove(KeyValuePairFast<TKey, TValue> item)
        {
            return Remove(item.Key);
        }
    }

    public struct ArrayMapKeyValueEnumerator<TKey, TValue> : IEnumerator<KeyValuePairFast<TKey, TValue>>
    {
        private readonly ArrayMap<TKey, TValue> _map;

#if ENABLE_DEBUG_CHECKS
        internal int _startCount;
#endif

        private int _count;
        private int _index;

        public ArrayMapKeyValueEnumerator(ArrayMap<TKey, TValue> map) : this()
        {
            _map = map;
            _index = -1;
            _count = map.Count;

#if ENABLE_DEBUG_CHECKS
            _startCount = map.Count;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
#if ENABLE_DEBUG_CHECKS
            if (_count != _startCount)
                throw new InvalidOperationException("Cannot modify a map while it is being iterated");
#endif

            if (_index >= _count - 1)
            {
                return false;
            }

            ++_index;
            return true;
        }

        public KeyValuePairFast<TKey, TValue> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_map._valuesInfo[_index].key, _map._values, _index);
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

#if ENABLE_DEBUG_CHECKS
            if (_count > _startCount)
                throw new InvalidOperationException("Cannot set a count greater than the starting one");

            _startCount = (int)count;
#endif
        }

        public void Reset()
        {
            _index = -1;
            _count = _map.Count;

#if ENABLE_DEBUG_CHECKS
            _startCount = _map.Count;
#endif
        }

        public void Dispose() { }
    }

    [DebuggerDisplay("[{Key}] = {Value}")]
    [DebuggerTypeProxy(typeof(KeyValuePairFastDebugProxy<,>))]
    public readonly struct KeyValuePairFast<TKey, TValue>
    {
        private readonly ManagedStrategy<TValue> _mapValues;
        private readonly TKey _key;
        private readonly int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePairFast(in TKey key, in ManagedStrategy<TValue> mapValues, int index)
        {
            _mapValues = mapValues;
            _index = index;
            _key = key;
        }

        public TKey Key
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _key;
        }

        public ref TValue Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _mapValues[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out TKey key, out TValue value)
        {
            key = Key;
            value = Value;
        }
    }

    internal sealed class KeyValuePairFastDebugProxy<TKey, TValue>
    {

        private readonly KeyValuePairFast<TKey, TValue> _keyValue;

        public KeyValuePairFastDebugProxy(in KeyValuePairFast<TKey, TValue> keyValue)
        {
            _keyValue = keyValue;
        }

        public TKey Key
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _keyValue.Key;
        }

        public TValue Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _keyValue.Value;
        }
    }

    internal sealed class ArrayMapDebugProxy<TKey, TValue>
    {

        private readonly ArrayMap<TKey, TValue> _map;

        public ArrayMapDebugProxy(in ArrayMap<TKey, TValue> map)
        {
            _map = map;
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (uint)_map.Count;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePairFast<TKey, TValue>[] KeyValues
        {
            get
            {
                var map = _map;
                var array = new KeyValuePairFast<TKey, TValue>[map.Count];
                var i = 0;

                foreach (var keyValue in map)
                {
                    array[i++] = keyValue;
                }

                return array;
            }
        }
    }
}
