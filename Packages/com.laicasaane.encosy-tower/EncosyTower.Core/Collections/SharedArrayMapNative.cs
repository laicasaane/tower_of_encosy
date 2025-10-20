// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/Dictionaries/SveltoDictionary.cs

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
using EncosyTower.Common;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections
{
    partial class SharedArrayMap<TKey, TValue, TValueNative>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SharedArrayMapNative<TKey, TValue, TValueNative> AsNative()
            => new(this);
    }

    /// <summary>
    /// This map has been created for just one reason: I needed a map that would have let me iterate
    /// over the values as an array, directly, without generating one or using an iterator.
    /// For this goal is N times faster than the standard Dictionary. This map is also faster than
    /// the standard Dictionary for most of the operations, but the difference is negligible. The only slower operation
    /// is resizing the memory on add, as this implementation needs to use two separate arrays compared to the standard
    /// one.
    /// </summary>
    /// <remarks>
    /// SharedArrayMapNative is not thread safe. A thread safe version should take care of possible setting of
    /// value with shared hash hence bucket list index.
    /// </remarks>
    public readonly partial struct SharedArrayMapNative<TKey, TValue, TValueNative> : IClearable
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        where TValueNative : unmanaged
    {
        internal readonly NativeArray<ArrayMapNode<TKey>> _valuesInfo;
        internal readonly NativeArray<TValueNative> _values;
        internal readonly NativeArray<int> _buckets;

        internal readonly NativeArray<int> _freeValueCellIndex;
        internal readonly NativeArray<uint> _collisions;
        internal readonly NativeArray<ulong> _fastModBucketsMultiplier;

        public SharedArrayMapNative([NotNull] SharedArrayMap<TKey, TValue, TValueNative> map)
        {
            _valuesInfo = map._valuesInfo.AsNativeArray();
            _values = map._values.AsNativeArray();
            _buckets = map._buckets.AsNativeArray();

            _freeValueCellIndex = map._freeValueCellIndex.AsNativeArray();
            _collisions = map._collisions.AsNativeArray();
            _fastModBucketsMultiplier = map._fastModBucketsMultiplier.AsNativeArray();
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buckets.IsCreated;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values.Length;
        }

        public readonly int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _freeValueCellIndex.AsReadOnlySpan()[0];
        }

        public KeyEnumerable Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(this);
        }

        public NativeSlice<TValueNative> Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values.Slice(0, _freeValueCellIndex.AsSpan()[0]);
        }

        public TValueNative this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _values.AsReadOnlySpan()[GetIndex(key)];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                AddValue(key, out var index);

                _values.AsSpan()[index] = value;
            }
        }

        /// <remarks>
        /// This returns readonly because the enumerator cannot be, but at the same time, it cannot be modified
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SharedArrayMapNativeKeyValueEnumerator<TKey, TValue, TValueNative> GetEnumerator()
            => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, in TValueNative value)
        {
            var itemAdded = AddValue(key, out var index);

#if __ENCOSY_VALIDATION__
            if (itemAdded == false)
            {
                ThrowHelper.ThrowInvalidOperationException_KeyPresent();
            }
            else
#else
            if (itemAdded)
#endif
            {
                _values.AsSpan()[index] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, in TValueNative value)
        {
            var itemAdded = AddValue(key, out var index);

            if (itemAdded)
                _values.AsSpan()[index] = value;

            return itemAdded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, in TValueNative value, out int index)
        {
            var itemAdded = AddValue(key, out index);

            if (itemAdded)
                _values.AsSpan()[index] = value;

            return itemAdded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(TKey key, in TValueNative value)
        {
            var itemAdded = AddValue(key, out var index);

#if __ENCOSY_VALIDATION__
            if (itemAdded)
            {
                ThrowHelper.ThrowInvalidOperationException_TrySetValueOnNotExistingKey();
            }
            else
#else
            if (itemAdded == false)
#endif
            {
                _values.AsSpan()[index] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            ref var freeValueCellIndex = ref _freeValueCellIndex.AsSpan()[0];

            if (freeValueCellIndex == 0)
                return;

            freeValueCellIndex = 0;

            // Buckets cannot be FastCleared because it's important that the values are reset to 0
            _buckets.AsSpan().Fill(default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key)
        {
            return TryFindIndex(key, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValueNative result)
        {
            if (TryFindIndex(key, out var findIndex))
            {
                result = _values.AsReadOnlySpan()[findIndex];
                return true;
            }

            result = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValueNative GetOrAdd(TKey key)
        {
            var values = _values.AsSpan();

            if (TryFindIndex(key, out var findIndex))
            {
                return ref values[findIndex];
            }

            AddValue(key, out findIndex);

            values[findIndex] = default;

            return ref values[findIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValueNative GetOrAdd(TKey key, out int index)
        {
            var values = _values.AsSpan();

            if (TryFindIndex(key, out index))
            {
                return ref values[index];
            }

            AddValue(key, out index);

            return ref values[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValueNative GetValueByRef(TKey key)
        {
            var found = TryFindIndex(key, out var findIndex);

            // Burst is not able to vectorise code if throw is found, regardless if it's actually ever thrown
#if __ENCOSY_VALIDATION__
            if (found == false)
            {
                ThrowHelper.ThrowKeyNotFoundException_KeyNotFound();
            }
#endif

            return ref _values.AsSpan()[findIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
            return Remove(key, out _, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key, out int index, out TValueNative value)
        {
            var buckets = _buckets.AsSpan();
            var valuesInfo = _valuesInfo.AsSpan();
            var values = _values.AsSpan();
            var fastModBucketsMultiplier = _fastModBucketsMultiplier.AsReadOnlySpan()[0];
            ref var freeValueCellIndex = ref _freeValueCellIndex.AsSpan()[0];

            var hash = key.GetHashCode();
            var bucketIndex = (int)Reduce((uint)hash, (uint)buckets.Length, fastModBucketsMultiplier);

            //find the bucket
            var indexToValueToRemove = buckets[bucketIndex] - 1;
            var itemAfterCurrentOne = -1;

            //Part one: look for the actual key in the bucket list if found I update the bucket list so that it doesn't
            //point anymore to the cell to remove
            while (indexToValueToRemove != -1)
            {
                ref var node = ref valuesInfo[indexToValueToRemove];

                if (node._hashcode == hash && key.Equals(node.key))
                {
                    //if the key is found and the bucket points directly to the node to remove
                    if (buckets[bucketIndex] - 1 == indexToValueToRemove)
                    {
                        //the bucket will point to the previous cell. if a previous cell exists
                        //its next pointer must be updated!
                        //<--- iteration order
                        //                      Bucket points always to the last one
                        //   ------- ------- -------
                        //   |  1  | |  2  | |  3  | //bucket cannot have next, only previous
                        //   ------- ------- -------
                        //--> insert order
                        buckets[bucketIndex] = node._previous + 1;
                    }
                    else //we need to update the previous pointer if it's not the last element that is removed
                    {
                        Checks.IsTrue(itemAfterCurrentOne != -1, "This should never happen");
                        //update the previous pointer of the item after the one to remove with the previous pointer of the item to remove
                        valuesInfo[itemAfterCurrentOne]._previous = node._previous;
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

            freeValueCellIndex--; //one less value to iterate
            value = values[indexToValueToRemove]; //value is a out variable, we want to know the value of the element to remove

            //Part two:
            //At this point nodes pointers and buckets are updated, but the _values array
            //still has got the value to delete. Remember the goal of this map is to be able
            //to iterate over the values like an array, so the values array must always be up to date

            //if the cell to remove is the last one in the list, we can perform less operations (no swapping needed)
            //otherwise we want to move the last value cell over the value to remove

            var lastValueCellIndex = freeValueCellIndex;

            if (indexToValueToRemove != lastValueCellIndex)
            {
                //we can transfer the last value of both arrays to the index of the value to remove.
                //in order to do so, we need to be sure that the bucket pointer is updated.
                //first we find the index in the bucket list of the pointer that points to the cell
                //to move
                ref var modeToMove = ref valuesInfo[lastValueCellIndex];

                var movingBucketIndex = (int)Reduce(
                      (uint)modeToMove._hashcode
                    , (uint)buckets.Length
                    , fastModBucketsMultiplier
                );

                var linkedListIterationIndex = buckets[movingBucketIndex] - 1;

                //if the key is found and the bucket points directly to the node to remove
                //it must now point to the cell where it's going to be moved (update bucket list first linked list node to iterate from)
                if (linkedListIterationIndex == lastValueCellIndex)
                    buckets[movingBucketIndex] = indexToValueToRemove + 1;

                //find the prev element of the last element in the valuesInfo array
                while (valuesInfo[linkedListIterationIndex]._previous != -1 && valuesInfo[linkedListIterationIndex]._previous != lastValueCellIndex)
                    linkedListIterationIndex = valuesInfo[linkedListIterationIndex]._previous;

                //if we find any value that has the last value cell as previous, we need to update it to point to the new value index that is going to be replaced
                if (valuesInfo[linkedListIterationIndex]._previous != -1)
                    valuesInfo[linkedListIterationIndex]._previous = indexToValueToRemove;

                //finally, actually move the values
                valuesInfo[indexToValueToRemove] = modeToMove;
                values[indexToValueToRemove] = values[lastValueCellIndex];
            }

            return true;
        }

        //I store all the index with an offset + 1, so that in the bucket list 0 means actually not existing.
        //When read the offset must be offset by -1 again to be the real one. In this way
        //I avoid to initialize the array to -1

        //WARNING this method must stay stateless (not relying on states that can change, it's ok to read
        //constant states) because it will be used in multithreaded parallel code
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindIndex(TKey key, out int findIndex)
        {
            var buckets = _buckets.AsReadOnlySpan();
            var valuesInfo = _valuesInfo.AsReadOnlySpan();
            var fastModBucketsMultiplier = _fastModBucketsMultiplier.AsReadOnlySpan()[0];

            Checks.IsTrue(buckets.Length > 0, "Map arrays are not correctly initialized (0 size)");

            var hash = key.GetHashCode();
            var bucketIndex = (int)Reduce((uint)hash, (uint)buckets.Length, fastModBucketsMultiplier);
            var valueIndex = buckets[bucketIndex] - 1;

            //even if we found an existing value we need to be sure it's the one we requested
            while (valueIndex != -1)
            {
                ref readonly var node = ref valuesInfo[valueIndex];

                if (node._hashcode == hash && key.Equals(node.key))
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Intersect<UValue, UValueNative>(in SharedArrayMapNative<TKey, UValue, UValueNative> otherMapKeys)
            where UValue : unmanaged
            where UValueNative : unmanaged
        {
            var keys = _valuesInfo.AsSpan();

            for (var i = Count - 1; i >= 0; i--)
            {
                var tKey = keys[i].key;

                if (otherMapKeys.ContainsKey(tKey) == false)
                {
                    Remove(tKey);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exclude<UValue, UValueNative>(in SharedArrayMapNative<TKey, UValue, UValueNative> otherMapKeys)
            where UValue : unmanaged
            where UValueNative : unmanaged
        {
            var keys = _valuesInfo.AsSpan();

            for (var i = Count - 1; i >= 0; i--)
            {
                var tKey = keys[i].key;

                if (otherMapKeys.ContainsKey(tKey))
                {
                    Remove(tKey);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Union(in SharedArrayMapNative<TKey, TValue, TValueNative> otherMapKeys)
        {
            foreach (var other in otherMapKeys)
            {
                this[other.Key] = other.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AddValue(TKey key, out int indexSet)
        {
            var valuesInfo = _valuesInfo.AsSpan();
            var buckets = _buckets.AsSpan();
            var fastModBucketsMultiplier = _fastModBucketsMultiplier.AsReadOnlySpan()[0];
            ref var freeValueCellIndex = ref _freeValueCellIndex.AsSpan()[0];
            ref var collisions = ref _collisions.AsSpan()[0];

            var hash = key.GetHashCode(); //IEquatable doesn't enforce the override of GetHashCode
            var bucketIndex = (int)Reduce((uint)hash, (uint)buckets.Length, fastModBucketsMultiplier);

            //buckets value -1 means it's empty
            var valueIndex = buckets[bucketIndex] - 1;

            if (valueIndex == -1)
            {
                ResizeIfNeeded();
                //create the info node at the last position and fill it with the relevant information
                valuesInfo[freeValueCellIndex] = new ArrayMapNode<TKey>(key, hash);
            }
            else //collision or already exists
            {
                var currentValueIndex = valueIndex;

                do
                {
                    //must check if the key already exists in the map
                    ref var node = ref valuesInfo[currentValueIndex];

                    if (node._hashcode == hash && key.Equals(node.key))
                    {
                        //the key already exists, simply replace the value!
                        indexSet = currentValueIndex;
                        return false;
                    }

                    currentValueIndex = node._previous;
                } while (currentValueIndex != -1); //-1 means no more values with key with the same hash

                ResizeIfNeeded();

                //oops collision!
                collisions++;

                //create a new node which previous index points to node currently pointed in the bucket (valueIndex)
                //_freeValueCellIndex = valueIndex + 1
                valuesInfo[freeValueCellIndex] = new ArrayMapNode<TKey>(key, hash, valueIndex);
                //Important: the new node is always the one that will be pointed by the bucket cell
                //so I can assume that the one pointed by the bucket is always the last value added
            }

            //item with this bucketIndex will point to the last value created
            //ToDo: if instead I assume that the original one is the one in the bucket
            //I wouldn't need to update the bucket here. Small optimization but important
            buckets[bucketIndex] = freeValueCellIndex + 1;

            indexSet = freeValueCellIndex;
            freeValueCellIndex++;

            //too many collisions
            if (collisions > buckets.Length)
            {
                if (buckets.Length < 100)
                    RecomputeBuckets((int)collisions << 1);
                else
                    RecomputeBuckets(HashHelpers.ExpandPrime((int)collisions));
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecomputeBuckets(int newSize)
        {
            ResizeBuckets(newSize);

            var valuesInfo = _valuesInfo.AsSpan();
            var buckets = _buckets.AsSpan();

            ref var collisions = ref _collisions.AsSpan()[0];
            var bucketsCapacity = (uint)buckets.Length;
            var fastModBucketsMultiplier = _fastModBucketsMultiplier.AsSpan()[0] = HashHelpers.GetFastModMultiplier(bucketsCapacity);

            //we need to get all the hash code of all the values stored so far and spread them over the new bucket
            //length
            var freeValueCellIndex = _freeValueCellIndex.AsSpan()[0];

            for (var newValueIndex = 0; newValueIndex < freeValueCellIndex; ++newValueIndex)
            {
                //get the original hash code and find the new bucketIndex due to the new length
                ref var valueInfoNode = ref valuesInfo[newValueIndex];
                var bucketIndex = (int)Reduce((uint)valueInfoNode._hashcode, bucketsCapacity, fastModBucketsMultiplier);

                //bucketsIndex can be -1 or a next value. If it's -1 means no collisions. If there is collision,
                //we create a new node which prev points to the old one. Old one next points to the new one.
                //the bucket will now points to the new one
                //In this way we can rebuild the linkedlist.
                //get the current valueIndex, it's -1 if no collision happens
                var existingValueIndex = buckets[bucketIndex] - 1;

                //update the bucket index to the index of the current item that share the bucketIndex
                //(last found is always the one in the bucket)
                buckets[bucketIndex] = newValueIndex + 1;

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
                    collisions++;
                    //the bucket will point to this value, so
                    //the previous index will be used as previous for the new value.
                    valueInfoNode._previous = existingValueIndex;
                }
            }
        }

        private void ResizeBuckets(int newSize)
        {
            Checks.IsTrue(newSize == _buckets.Length, "the capacity of SharedArrayMapNative<TKey, TValue> is immutable and cannot change");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeIfNeeded()
        {
            Checks.IsTrue(Count != _values.Length, "the capacity of SharedArrayMapNative<TKey, TValue> is immutable and cannot change");
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
            private readonly SharedArrayMapNative< TKey, TValue, TValueNative > _map;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyEnumerable(in SharedArrayMapNative<TKey, TValue, TValueNative> map)
            {
                _map = map;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyEnumerator GetEnumerator()
                => new(_map);
        }

        public struct KeyEnumerator
        {
            private readonly SharedArrayMapNative< TKey, TValue, TValueNative > _map;
            private readonly int _count;

            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyEnumerator(in SharedArrayMapNative<TKey, TValue, TValueNative> map) : this()
            {
                _map = map;
                _index = -1;
                _count = map.Count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
#if __ENCOSY_VALIDATION__
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

            public readonly TKey Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map._valuesInfo.AsReadOnlySpan()[_index].key;
            }
        }
    }

    public struct SharedArrayMapNativeKeyValueEnumerator<TKey, TValue, TValueNative>
        : IEnumerator<SharedArrayMapNativeKeyValuePairFast<TKey, TValue, TValueNative>>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        where TValueNative : unmanaged
    {
        private readonly SharedArrayMapNative<TKey, TValue, TValueNative> _map;

#if __ENCOSY_VALIDATION__
        internal int _startCount;
#endif

        private int _count;
        private int _index;

        public SharedArrayMapNativeKeyValueEnumerator(SharedArrayMapNative<TKey, TValue, TValueNative> map) : this()
        {
            _map = map;
            _index = -1;
            _count = map.Count;

#if __ENCOSY_VALIDATION__
            _startCount = map.Count;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
#if __ENCOSY_VALIDATION__
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

        public readonly SharedArrayMapNativeKeyValuePairFast<TKey, TValue, TValueNative> Current
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

    public readonly struct SharedArrayMapNativeKeyValuePairFast<TKey, TValue, TValueNative>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        where TValueNative : unmanaged
    {
        private readonly NativeArray<TValueNative> _mapValues;
        private readonly TKey _key;
        private readonly int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SharedArrayMapNativeKeyValuePairFast(in TKey key, NativeArray<TValueNative> mapValues, int index)
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

        public readonly ref readonly TValueNative Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _mapValues.AsReadOnlySpan()[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out TKey key, out TValueNative value)
        {
            key = Key;
            value = Value;
        }
    }
}
