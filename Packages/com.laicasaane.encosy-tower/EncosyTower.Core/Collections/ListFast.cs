// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/Arrays/ListFast.cs

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic.Unsafe;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Debugging;

namespace EncosyTower.Collections
{
    public readonly struct ListFast<T> : IList<T>, IReadOnlyList<T>
        , IAsSpan<T>, IAsReadOnlySpan<T>
        , IClearable, IHasCapacity
    {
        internal static readonly EqualityComparer<T> s_comp = EqualityComparer<T>.Default;
        internal static readonly bool s_shouldPerformMemClear = RuntimeHelpers.IsReferenceOrContainsReferences<T>();

        internal readonly ListExposed<T> _list;

        public ListFast([NotNull] List<T> list)
        {
            _list = new(list);
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _list.List != null;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Length;
        }

        public bool IsReadOnly => false;

        public List<T> List
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _list.List;
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Checks.IsTrue((uint)index < (uint)_count, "index is outside the range of valid indexes for the ListFast<T>");
                return _buffer[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Checks.IsTrue((uint)index < (uint)_count, "index is outside the range of valid indexes for the ListFast<T>");
                _version++;
                _buffer[index] = value;
            }
        }

#pragma warning disable IDE1006 // Naming Styles
        internal ref T[] _buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _list.Item;
        }

        internal ref int _count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _list.Size;
        }

        internal ref int _version
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _list.Version;
        }
#pragma warning restore IDE1006 // Naming Styles

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists([NotNull] Predicate<T> match)
            => FindIndex(match) != -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists([NotNull] PredicateIn<T> match)
            => FindIndex(match) != -1;

        public Option<T> Find([NotNull] Predicate<T> match)
        {
            var items = AsReadOnlySpan();
            var length = items.Length;

            for (var i = 0; i < length; i++)
            {
                ref readonly var item = ref items[i];

                if (match(item))
                    return item;
            }

            return Option.None;
        }

        public Option<T> Find([NotNull] PredicateIn<T> match)
        {
            var items = AsReadOnlySpan();
            var length = items.Length;

            for (var i = 0; i < length; i++)
            {
                ref readonly var item = ref items[i];

                if (match(in item))
                    return item;
            }

            return Option.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ListFast<T> FindAll([NotNull] Predicate<T> match)
        {
            var result = new ListFast<T>();
            FindAll(match, result);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ListFast<T> FindAll([NotNull] PredicateIn<T> match)
        {
            var result = new ListFast<T>();
            FindAll(match, result);

            return result;
        }

        public void FindAll([NotNull] Predicate<T> match, [NotNull] ListFast<T> result)
        {
            var items = AsReadOnlySpan();
            var length = items.Length;

            for (var i = 0; i < length; i++)
            {
                ref readonly var item = ref items[i];

                if (match(item))
                {
                    result.Add(item);
                }
            }
        }

        public void FindAll([NotNull] PredicateIn<T> match, [NotNull] ListFast<T> result)
        {
            var items = AsReadOnlySpan();
            var length = items.Length;

            for (var i = 0; i < length; i++)
            {
                ref readonly var item = ref items[i];

                if (match(in item))
                {
                    result.Add(in item);
                }
            }
        }

        public void FindAll([NotNull] Predicate<T> match, [NotNull] ICollection<T> result)
        {
            if (result is ListFast<T> fasterResult)
            {
                FindAll(match, fasterResult);
                return;
            }

            var items = AsReadOnlySpan();
            var length = items.Length;

            for (var i = 0; i < length; i++)
            {
                ref readonly var item = ref items[i];

                if (match(item))
                {
                    result.Add(item);
                }
            }
        }

        public void FindAll([NotNull] PredicateIn<T> match, [NotNull] ICollection<T> result)
        {
            if (result is ListFast<T> fasterResult)
            {
                FindAll(match, fasterResult);
                return;
            }

            var items = AsReadOnlySpan();
            var length = items.Length;

            for (var i = 0; i < length; i++)
            {
                ref readonly var item = ref items[i];

                if (match(in item))
                {
                    result.Add(item);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindIndex([NotNull] Predicate<T> match)
            => FindIndex(0, _count, match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindIndex(int startIndex, Predicate<T> match)
            => FindIndex(startIndex, _count - startIndex, match);

        public int FindIndex(int startIndex, int count, [NotNull] Predicate<T> match)
        {
            Checks.IsTrue((uint)startIndex < (uint)_count, "startIndex is outside the range of valid indexes for the ListFast<T>");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(startIndex < _count - count, "startIndex and count do not specify a valid section in the ListFast<T>");

            var items = AsReadOnlySpan();
            var endIndex = startIndex + count;

            for (var i = startIndex; i < endIndex; i++)
            {
                ref readonly var item = ref items[i];

                if (match(item))
                    return i;
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindIndex([NotNull] PredicateIn<T> match)
            => FindIndex(0, _count, match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindIndex(int startIndex, PredicateIn<T> match)
            => FindIndex(startIndex, _count - startIndex, match);

        public int FindIndex(int startIndex, int count, [NotNull] PredicateIn<T> match)
        {
            Checks.IsTrue((uint)startIndex < (uint)_count, "startIndex is outside the range of valid indexes for the ListFast<T>");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(startIndex < _count - count, "startIndex and count do not specify a valid section in the ListFast<T>");

            var items = AsReadOnlySpan();
            var endIndex = startIndex + count;

            for (var i = startIndex; i < endIndex; i++)
            {
                ref readonly var item = ref items[i];

                if (match(in item))
                    return i;
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindLastIndex([NotNull] Predicate<T> match)
            => FindLastIndex(_count - 1, _count, match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindLastIndex(int startIndex, [NotNull] Predicate<T> match)
            => FindLastIndex(startIndex, startIndex + 1, match);

        public int FindLastIndex(int startIndex, int count, [NotNull] Predicate<T> match)
        {
            if (_count == 0)
            {
                Checks.IsTrue(startIndex == -1, "startIndex is outside the range of valid indexes for the ListFast<T>");
            }
            else
            {
                Checks.IsTrue((uint)startIndex < (uint)_count, "startIndex is outside the range of valid indexes for the ListFast<T>");
            }

            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(startIndex - count + 1 >= 0, "startIndex and count do not specify a valid section in the ListFast<T>");

            var items = AsReadOnlySpan();
            var endIndex = startIndex - count;

            for (var i = startIndex; i > endIndex; i--)
            {
                ref readonly var item = ref items[i];

                if (match(item))
                {
                    return i;
                }
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindLastIndex([NotNull] PredicateIn<T> match)
            => FindLastIndex(_count - 1, _count, match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindLastIndex(int startIndex, [NotNull] PredicateIn<T> match)
            => FindLastIndex(startIndex, startIndex + 1, match);

        public int FindLastIndex(int startIndex, int count, [NotNull] PredicateIn<T> match)
        {
            if (_count == 0)
            {
                Checks.IsTrue(startIndex == -1, "startIndex is outside the range of valid indexes for the ListFast<T>");
            }
            else
            {
                Checks.IsTrue((uint)startIndex < (uint)_count, "startIndex is outside the range of valid indexes for the ListFast<T>");
            }

            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(startIndex - count + 1 >= 0, "startIndex and count do not specify a valid section in the ListFast<T>");

            var items = AsReadOnlySpan();
            var endIndex = startIndex - count;

            for (var i = startIndex; i > endIndex; i--)
            {
                ref readonly var item = ref items[i];

                if (match(in item))
                {
                    return i;
                }
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item)
            => IndexOf(item, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item, int index)
            => IndexOf(item, index, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item, int index, int count)
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(index + count <= _count, "index and count do not specify a valid section in the ListFast<T>");
            return Array.IndexOf(_buffer, item, index, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item)
            => IndexOf(in item, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index)
            => IndexOf(in item, index, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index, int count)
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(index + count <= _count, "index and count do not specify a valid section in the ListFast<T>");
            return Array.IndexOf(_buffer, item, index, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            _version++;

            if (_count == _buffer.Length)
                AllocateMore();

            _buffer[_count++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item)
        {
            _version++;

            if (_count == _buffer.Length)
                AllocateMore();

            _buffer[_count++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, T item)
        {
            Checks.IsTrue((uint)index < (uint)_count, "index is outside the range of valid indexes for the ListFast<T>");

            _version++;

            if (_count == _buffer.Length)
                AllocateMore();

            Array.Copy(_buffer, index, _buffer, index + 1, _count - index);
            ++_count;

            _buffer[index] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in T item)
        {
            Checks.IsTrue((uint)index < (uint)_count, "index is outside the range of valid indexes for the ListFast<T>");

            _version++;

            if (_count == _buffer.Length)
                AllocateMore();

            Array.Copy(_buffer, index, _buffer, index + 1, _count - index);
            ++_count;

            _buffer[index] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ElementAt(int index)
        {
            Checks.IsTrue((uint)index < (uint)_count, "index is outside the range of valid indexes for the ListFast<T>");
            return ref _buffer[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(in ReadOnlyList<T> items)
        {
            _version++;
            AddRange(items._list._buffer, items.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange([NotNull] T[] items)
            => AddRange(items, items.Length);

        public void AddRange([NotNull] T[] items, int count)
        {
            _version++;

            if (count == 0) return;

            if (_buffer.Length - _count < count)
                AllocateMore(checked(_count + count));

            Array.Copy(items, 0, _buffer, _count, count);
            _count += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(ReadOnlySpan<T> items)
            => AddRange(items, items.Length);

        public void AddRange(ReadOnlySpan<T> items, int count)
        {
            _version++;

            if (count == 0) return;

            if (_buffer.Length - _count < count)
                AllocateMore(checked(_count + count));

            items[..count].CopyTo(_buffer.AsSpan(_count, count));
            _count += count;
        }

        public void AddRange([NotNull] IEnumerable<T> collection)
        {
            if (collection is ICollection<T> c)
            {
                var count = c.Count;

                if (count > 0)
                {
                    if (_buffer.Length - _count < count)
                    {
                        AllocateMore(checked(_count + count));
                    }

                    c.CopyTo(_buffer, _count);
                    _count += count;
                    _version++;
                }
            }
            else
            {
                using IEnumerator<T> en = collection.GetEnumerator();

                while (en.MoveNext())
                {
                    Add(en.Current);
                }
            }
        }

        public bool Contains(T item)
        {
            var items = AsReadOnlySpan();
            var length = items.Length;
            var comp = s_comp;

            for (var index = 0; index < length; index++)
            {
                ref readonly var item2 = ref items[index];

                if (comp.Equals(item2, item))
                    return true;
            }

            return false;
        }

        public bool Contains(in T item)
        {
            var items = AsReadOnlySpan();
            var length = items.Length;
            var comp = s_comp;

            for (var index = 0; index < length; index++)
            {
                ref readonly var item2 = ref items[index];

                if (comp.Equals(item2, item))
                    return true;
            }

            return false;
        }

        public void ForEach([NotNull] Action<T> action)
        {
            var items = AsReadOnlySpan();
            var length = items.Length;
            var version = _version;

            for (var i = 0; i < length; i++)
            {
                if (version != _version)
                {
                    break;
                }

                ref readonly var item = ref items[i];
                action(item);
            }

            Checks.IsTrue(version == _version, "An element in the collection has been modified.");
        }

        public void ForEach([NotNull] ActionIn<T> action)
        {
            var items = AsReadOnlySpan();
            var length = items.Length;
            var version = _version;

            for (var i = 0; i < length; i++)
            {
                if (version != _version)
                {
                    break;
                }

                ref readonly var item = ref items[i];
                action(in item);
            }

            Checks.IsTrue(version == _version, "An element in the collection has been modified.");
        }

        public void ForEach([NotNull] ActionRef<T> action)
        {
            var items = AsSpan();
            var length = items.Length;
            var version = _version;

            for (var i = 0; i < length; i++)
            {
                if (version != _version)
                {
                    break;
                }

                action(ref items[i]);
            }

            Checks.IsTrue(version == _version, "An element in the collection has been modified.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array)
            => Array.Copy(_buffer, array, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
            => Array.Copy(_buffer, 0, array, arrayIndex, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, T[] array, int arrayIndex, int length)
            => Array.Copy(_buffer, index, array, arrayIndex, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> array)
            => CopyTo(0, array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, Span<T> array)
            => CopyTo(index, array, array.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, Span<T> array, int length)
            => AsReadOnlySpan().Slice(index, length).CopyTo(array[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _version++;

            if (s_shouldPerformMemClear)
            {
                Array.Clear(_buffer, 0, _buffer.Length);
            }

            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ListFastEnumerator<T> GetEnumerator()
            => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncreaseCapacityBy(int amount)
            => IncreaseCapacityTo(_buffer.Length + amount);

        public void IncreaseCapacityTo(int newCapacity)
        {
            _version++;

            if (newCapacity <= _buffer.Length)
            {
                return;
            }

            var newList = new T[newCapacity];
            if (_count > 0) Array.Copy(_buffer, newList, _count);
            _buffer = newList;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Peek()
            => ref _buffer[_count - 1];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Pop()
        {
            _version++;
            --_count;
            return ref _buffer[_count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Push(T item)
        {
            Insert(_count, item);
            return _count - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Push(in T item)
        {
            Insert(_count, item);
            return _count - 1;
        }

        public bool Remove(T item)
        {
            _version++;

            var index = IndexOf(item);

            if ((uint)index >= (uint)_count)
                return false;

            if (index < --_count)
            {
                Array.Copy(_buffer, index + 1, _buffer, index, _count - index);
            }

            if (s_shouldPerformMemClear)
            {
                _buffer[_count] = default;
            }

            return true;
        }

        public bool Remove(in T item)
        {
            _version++;

            var index = IndexOf(in item);

            if ((uint)index >= (uint)_count)
                return false;

            if (index < --_count)
            {
                Array.Copy(_buffer, index + 1, _buffer, index, _count - index);
            }

            if (s_shouldPerformMemClear)
            {
                _buffer[_count] = default;
            }

            return true;
        }

        public void RemoveAt(int index)
        {
            _version++;

            Checks.IsTrue((uint)index < (uint)_count, "out of bound index");

            if (index < --_count)
            {
                Array.Copy(_buffer, index + 1, _buffer, index, _count - index);
            }

            if (s_shouldPerformMemClear)
            {
                _buffer[_count] = default;
            }
        }

        public void RemoveRange(int startIndex, int length)
        {
            _version++;

            var count = _count;

            Checks.IsTrue((uint)startIndex < (uint)count, "out of bound start index");

            var end = startIndex + length;

            Checks.IsTrue((uint)end <= (uint)count, "out of bound length");

            if (length < 1)
            {
                return;
            }

            count = _count -= length;

            if (startIndex < count)
            {
                Array.Copy(_buffer, startIndex + length, _buffer, startIndex, count - startIndex);
            }

            if (s_shouldPerformMemClear)
            {
                Array.Clear(_buffer, count, length);
            }
        }

        public void RemoveAtSwapBack(int index)
        {
            _version++;

            Checks.IsTrue((uint)index < (uint)_count, "out of bound index");

            if (index < --_count)
            {
                _buffer[index] = _buffer[_count];
            }

            if (s_shouldPerformMemClear)
            {
                _buffer[_count] = default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize)
        {
            _version++;

            if (newSize == _buffer.Length) return;

            Array.Resize(ref _buffer, newSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
            => AsReadOnlySpan().ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            _version++;
            return _buffer.AsSpan(0, _count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
            => _buffer.AsSpan(0, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            if (_count < _buffer.Length)
                Resize(_count);
        }

        public void AddReplicate<TDerived>(int amount)
            where TDerived : T, new()
        {
            _version++;

            var oldCount = _count;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Length;

            if (offset > 0)
            {
                AllocateMore(newCount);
            }

            if (default(TDerived) == null)
            {
                var buffer = _buffer.AsSpan().Slice(oldCount, amount);

                for (var i = 0; i < amount; i++)
                {
                    buffer[i] = new TDerived();
                }
            }

            _count = newCount;
        }

        public void AddReplicate(int amount)
        {
            _version++;

            var oldCount = _count;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Length;

            if (offset > 0)
            {
                AllocateMore(newCount);
            }

            _buffer.AsSpan().Slice(oldCount, amount).Fill(default);
            _count = newCount;
        }

        public void AddReplicate(T value, int amount)
        {
            _version++;

            var oldCount = _count;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Length;

            if (offset > 0)
            {
                AllocateMore(newCount);
            }

            _buffer.AsSpan().Slice(oldCount, amount).Fill(value);
            _count = newCount;
        }

        public void AddReplicateNoInit(int amount)
        {
            _version++;

            var oldCount = _count;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Length;

            if (offset > 0)
            {
                AllocateMore(newCount);
            }

            _count = newCount;
        }

        /// <summary>
        /// Sorts the elements in this list. Uses the default comparer and Array.Sort.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort()
            => Sort(0, _count, Comparer<T>.Default);

        /// <summary>
        /// Sorts the elements in this list. Uses Array.Sort with the provided comparer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort([NotNull] IComparer<T> comparer)
            => Sort(0, _count, comparer);

        /// <summary>
        /// Sorts the elements in a section of this list. The sort compares the
        /// elements to each other using the given IComparer interface. If
        /// comparer is null, the elements are compared to each other using
        /// the IComparable interface, which in that case must be implemented by all
        /// elements of the list.
        /// <br/>
        /// This method uses the Array.Sort method to sort the elements.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(int index, int count, [NotNull] IComparer<T> comparer)
        {
            Checks.IsTrue(index >= 0, "'index' must be non-negative number");
            Checks.IsTrue(count >= 0, "'count' must be non-negative number");
            Checks.IsTrue(_count - index >= count, "Invalid offset length");

            if (count > 1)
            {
                Array.Sort(_buffer, index, count, comparer);
            }

            _version++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort([NotNull] Comparison<T> comparison)
        {
            if (_count > 1)
            {
                Array.Sort(_buffer, 0, _count, Comparer<T>.Create(comparison));
            }

            _version++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListFast<T> Prefill<TDerived>(int amount)
            where TDerived : T, new()
        {
            var list = new ListFast<T>(new List<T>(amount));
            list.AddReplicate<TDerived>(amount);
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListFast<T> Prefill(int amount)
        {
            var list = new ListFast<T>(new List<T>(amount));
            list.AddReplicate(amount);
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListFast<T> Prefill(T value, int amount)
        {
            var list = new ListFast<T>(new List<T>(amount));
            list.AddReplicate(value, amount);
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListFast<T> PrefillNoInit(int amount)
        {
            var list = new ListFast<T>(new List<T>(amount));
            list.AddReplicateNoInit(amount);
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ListFast<T>([NotNull] List<T> list)
            => new(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int CalcNewCapacity(int newSize)
        {
            newSize = Math.Max(4, newSize);
            return ((int)Math.Ceiling(newSize * 1.5f) / 4) * 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AllocateMore()
        {
            var newCapacity = CalcNewCapacity(_buffer.Length + 1);
            var newList = new T[newCapacity];
            if (_count > 0) Array.Copy(_buffer, newList, _count);
            _buffer = newList;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AllocateMore(int newSize)
        {
            Checks.IsTrue(newSize > _buffer.Length, "newSize is not greater than the current capacity");

            var newCapacity = CalcNewCapacity(newSize);
            var newList = new T[newCapacity];
            if (_count > 0) Array.Copy(_buffer, newList, _count);
            _buffer = newList;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => new ListFastEnumerator<T>(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => new ListFastEnumerator<T>(this);
    }
}
