using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Module.Core.Buffers;

namespace Module.Core.Collections
{
    /// <summary>
    /// 'Stateless' means the list does not own an internal buffer and related data.
    /// Instead, it relies on an <see cref="IBufferProvider{T}"/> to have access to those data.
    /// Effectively, anything implementing <see cref="IBufferProvider{T}"/> can be used
    /// as the external state for this list.
    /// </summary>
    public class StatelessList<TState, T> : ICollection<T>, IReadOnlyCollection<T>, IList<T>, IReadOnlyList<T>
        , IAsSpan<T>, IAsReadOnlySpan<T>, IAsMemory<T>, IAsReadOnlyMemory<T>
        where TState : IBufferProvider<T>
    {
        internal static readonly bool s_shouldPerformMemClear = TypeCache<T>.IsUnmanaged == false;

        public readonly TState State;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatelessList([NotNull] TState state)
        {
            this.State = state;
        }

        public StatelessList([NotNull] TState state, int capacity)
        {
            this.State = state;

            if (capacity > this.State.Buffer.Length)
            {
                AllocateMore(capacity);
            }
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => State != null;
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Ensures compatibility with StatelessList<TState, T>")]
        internal ref T[] _buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Checks.IsTrue(State != null, "StatelessList<T> is not initialized");
                return ref State.Buffer;
            }
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Ensures compatibility with StatelessList<TState, T>")]
        internal ref int _count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Checks.IsTrue(State != null, "StatelessList<T> is not initialized");
                return ref State.Count;
            }
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Ensures compatibility with StatelessList<TState, T>")]
        internal ref int _version
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Checks.IsTrue(State != null, "StatelessList<T> is not initialized");
                return ref State.Version;
            }
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

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Checks.IsTrue((uint)index < (uint)_count, "index is outside the range of valid indexes for the StatelessList<TState, T>");
                return _buffer[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Checks.IsTrue((uint)index < (uint)_count, "index is outside the range of valid indexes for the StatelessList<TState, T>");
                _version++;
                _buffer[index] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BinarySearch(T item)
            => BinarySearch(0, Count, item, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BinarySearch(T item, IComparer<T> comparer)
            => BinarySearch(0, Count, item, comparer);

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(_count - index >= count, "index and count do not denote a valid range in the StatelessList<TState, T>");

            return AsReadOnlySpan().BinarySearch(item, comparer ?? Comparer<T>.Default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BinarySearch(in T item)
            => BinarySearch(0, Count, in item, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BinarySearch(in T item, IComparer<T> comparer)
            => BinarySearch(0, Count, in item, comparer);

        public int BinarySearch(int index, int count, in T item, IComparer<T> comparer)
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(_count - index >= count, "index and count do not denote a valid range in the StatelessList<TState, T>");

            return AsReadOnlySpan().BinarySearch(item, comparer ?? Comparer<T>.Default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists([NotNull] Predicate<T> match)
            => FindIndex(match) != -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists([NotNull] PredicateIn<T> match)
            => FindIndex(match) != -1;

        public T Find([NotNull] Predicate<T> match)
        {
            var items = AsReadOnlySpan();
            var length = items.Length;

            for (var i = 0; i < length; i++)
            {
                ref readonly var item = ref items[i];

                if (match(item))
                    return item;
            }

            return default;
        }

        public T Find([NotNull] PredicateIn<T> match)
        {
            var items = AsReadOnlySpan();
            var length = items.Length;

            for (var i = 0; i < length; i++)
            {
                ref readonly var item = ref items[i];

                if (match(in item))
                    return item;
            }

            return default;
        }

        public FasterList<T> FindAll([NotNull] Predicate<T> match)
        {
            var items = AsReadOnlySpan();
            var length = items.Length;
            var result = new FasterList<T>();

            for (var i = 0; i < length; i++)
            {
                ref readonly var item = ref items[i];

                if (match(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public FasterList<T> FindAll([NotNull] PredicateIn<T> match)
        {
            var items = AsReadOnlySpan();
            var length = items.Length;
            var result = new FasterList<T>();

            for (var i = 0; i < length; i++)
            {
                ref readonly var item = ref items[i];

                if (match(in item))
                {
                    result.Add(in item);
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindIndex([NotNull] Predicate<T> match)
            => FindIndex(0, _count, match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindIndex(int startIndex, Predicate<T> match)
            => FindIndex(startIndex, _count - startIndex, match);

        public int FindIndex(int startIndex, int count, [NotNull] Predicate<T> match)
        {
            Checks.IsTrue((uint)startIndex < (uint)_count, "startIndex is outside the range of valid indexes for the StatelessList<TState, T>");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(startIndex < _count - count, "startIndex and count do not specify a valid section in the StatelessList<TState, T>");

            var items = AsReadOnlySpan();
            var endIndex = startIndex + count;

            for (int i = startIndex; i < endIndex; i++)
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
            Checks.IsTrue((uint)startIndex < (uint)_count, "startIndex is outside the range of valid indexes for the StatelessList<TState, T>");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(startIndex < _count - count, "startIndex and count do not specify a valid section in the StatelessList<TState, T>");

            var items = AsReadOnlySpan();
            var endIndex = startIndex + count;

            for (int i = startIndex; i < endIndex; i++)
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
                Checks.IsTrue(startIndex == -1, "startIndex is outside the range of valid indexes for the StatelessList<TState, T>");
            }
            else
            {
                Checks.IsTrue((uint)startIndex < (uint)_count, "startIndex is outside the range of valid indexes for the StatelessList<TState, T>");
            }

            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(startIndex - count + 1 >= 0, "startIndex and count do not specify a valid section in the StatelessList<TState, T>");

            var items = AsReadOnlySpan();
            var endIndex = startIndex - count;

            for (int i = startIndex; i > endIndex; i--)
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
                Checks.IsTrue(startIndex == -1, "startIndex is outside the range of valid indexes for the StatelessList<TState, T>");
            }
            else
            {
                Checks.IsTrue((uint)startIndex < (uint)_count, "startIndex is outside the range of valid indexes for the StatelessList<TState, T>");
            }

            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(startIndex - count + 1 >= 0, "startIndex and count do not specify a valid section in the StatelessList<TState, T>");

            var items = AsReadOnlySpan();
            var endIndex = startIndex - count;

            for (int i = startIndex; i > endIndex; i--)
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
            => IndexOf(item, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item, IComparer<T> comparer)
            => AsReadOnlySpan().BinarySearch(item, comparer ?? Comparer<T>.Default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item, int index)
            => IndexOf(item, index, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item, int index, int count)
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(index + count <= _count, "index and count do not specify a valid section in the StatelessList<TState, T>");
            return Array.IndexOf(_buffer, item, index, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item)
            => IndexOf(in item, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, IComparer<T> comparer)
            => AsReadOnlySpan().BinarySearch(item, comparer ?? Comparer<T>.Default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index)
            => IndexOf(in item, index, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index, int count)
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(index + count <= _count, "index and count do not specify a valid section in the StatelessList<TState, T>");
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
            _version++;
            EnsureCountIsAtLeast(index + 1);
            _buffer[index] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in T item)
        {
            _version++;
            EnsureCountIsAtLeast(index + 1);
            _buffer[index] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ElementAt(int index)
        {
            Checks.IsTrue((uint)index < (uint)_count, "index is outside the range of valid indexes for the StatelessList<TState, T>");
            return ref _buffer[index];
        }

        public ref T GetOrCreateAt(int index, [NotNull] Func<T> item)
        {
            _version++;

            EnsureCountIsAtLeast(index + 1);

            if (EqualityComparer<T>.Default.Equals(this[index], default) == true)
                this[index] = item();

            return ref ElementAt(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange<UState>([NotNull] StatelessList<UState, T> items)
            where UState : IBufferProvider<T>
        {
            _version++;
            AddRange(items._buffer, items._count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange([NotNull] FasterList<T> items)
        {
            _version++;
            AddRange(items._buffer, items._count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(in FasterReadOnlyList<T> items)
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
                int count = c.Count;

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
            var comp = EqualityComparer<T>.Default;

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
            var comp = EqualityComparer<T>.Default;

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
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
            => Array.Copy(_buffer, index, array, arrayIndex, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> array)
            => AsReadOnlySpan().CopyTo(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, Span<T> array)
            => AsReadOnlySpan()[index..].CopyTo(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, int count, Span<T> array)
            => AsReadOnlySpan().Slice(index, count).CopyTo(array);

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
        public FasterListEnumerator<T> GetEnumerator()
            => new(_buffer, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncreaseCapacityBy(int increment)
            => IncreaseCapacityTo(_buffer.Length + increment);

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

        public void SetCountTo(int newCount)
        {
            _version++;

            if (_buffer.Length < newCount)
                AllocateMore(newCount);

            _count = newCount;
        }

        public void EnsureCountIsAtLeast(int newCount)
        {
            _version++;

            if (_buffer.Length < newCount)
                AllocateMore(newCount);

            if (_count < newCount)
                _count = newCount;
        }

        public void IncrementCountBy(int increment)
        {
            _version++;

            var count = _count + increment;

            if (_buffer.Length < count)
                AllocateMore(count);

            _count = count;
        }

        public void InsertAt(uint index, T item)
        {
            _version++;

            Checks.IsTrue(index <= _count, "out of bound index");

            if (_count == _buffer.Length) AllocateMore();

            Array.Copy(_buffer, index, _buffer, index + 1, _count - index);
            ++_count;

            _buffer[index] = item;
        }

        public void InsertAt(uint index, in T item)
        {
            _version++;

            Checks.IsTrue(index <= _count, "out of bound index");

            if (_count == _buffer.Length) AllocateMore();

            Array.Copy(_buffer, index, _buffer, index + 1, _count - index);
            ++_count;

            _buffer[index] = item;
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

            var index = AsSpan().BinarySearch(item, Comparer<T>.Default);

            if (index < 0)
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

            Checks.IsTrue(index < _count, "out of bound index");

            if (index < --_count)
            {
                Array.Copy(_buffer, index + 1, _buffer, index, _count - index);
            }

            if (s_shouldPerformMemClear)
            {
                _buffer[_count] = default;
            }
        }

        public void RemoveAt(int startIndex, int length)
        {
            _version++;

            var count = _count;

            Checks.IsTrue(startIndex < count, "out of bound start index");

            var end = startIndex + length;

            Checks.IsTrue(end <= count, "out of bound length");

            _count -= length;

            if (end == count)
            {
                if (s_shouldPerformMemClear)
                {
                    Array.Clear(_buffer, startIndex, length);
                }

                return;
            }

            length = count - end;
            Array.Copy(_buffer, end, _buffer, startIndex, length);

            if (s_shouldPerformMemClear)
            {
                startIndex += length;
                Array.Clear(_buffer, startIndex, count - startIndex);
            }
        }

        public void RemoveAtSwapBack(int index)
        {
            _version++;

            Checks.IsTrue(index < _count, "out of bound index");

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
        public void ResetToReuse()
        {
            _version++;
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize)
        {
            _version++;

            if (newSize == _buffer.Length) return;

            Array.Resize(ref _buffer, newSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReuseOneSlot()
        {
            _version++;

            if (_count >= _buffer.Length)
                return false;

            _count++;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
            => AsReadOnlySpan().ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
            => _buffer.AsSpan(0, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
            => _buffer.AsSpan(0, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory()
            => _buffer.AsMemory(0, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> AsReadOnlyMemory()
            => _buffer.AsMemory(0, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            if (_count < _buffer.Length)
                Resize(_count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimCount(int newCount)
        {
            _version++;

            Checks.IsTrue(_count >= newCount, "the new length must be less than the current one");

            _count = newCount;
        }

        public bool UnorderedRemoveAt(int index)
        {
            _version++;

            Checks.IsTrue(index < _count && _count > 0, "out of bound index");

            if (index == --_count)
            {
                _buffer[_count] = default;
                return false;
            }

            _buffer[index] = _buffer[_count];
            _buffer[_count] = default;

            return true;
        }

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

        public bool ReuseOneSlot<TDerived>(out TDerived result)
            where TDerived : T
        {
            _version++;

            if (_count >= _buffer.Length)
            {
                result = default;

                return false;
            }

            if (default(TDerived) == null)
            {
                result = (TDerived)_buffer[_count];

                if (result != null)
                {
                    _count++;
                    return true;
                }

                return false;
            }

            _count++;
            result = default;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReuseOneSlot<TDerived>()
            where TDerived : T
        {
            _version++;

            if (_count >= _buffer.Length)
                return false;

            _count++;

            return true;
        }

        public void AddReplicate<TDerived>(int count)
            where TDerived : T, new()
        {
            _version++;

            var oldCount = _count;
            var newCount = count + oldCount;
            var offset = newCount - _buffer.Length;

            if (offset > 0)
            {
                AllocateMore(newCount);
            }

            if (default(TDerived) == null)
            {
                for (var i = 0; i < count; i++)
                {
                    _buffer[i + oldCount] = new TDerived();
                }
            }

            _count = newCount;
        }

        public void AddReplicate(T value, int count)
        {
            _version++;

            var oldCount = _count;
            var newCount = count + oldCount;
            var offset = newCount - _buffer.Length;

            if (offset > 0)
            {
                AllocateMore(newCount);
            }

            for (var i = 0; i < count; i++)
            {
                _buffer[i + oldCount] = value;
            }

            _count = newCount;
        }

        public void AddReplicateNoInit(int count)
        {
            _version++;

            var oldCount = _count;
            var newCount = count + oldCount;
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
            => Sort(0, Count, null);

        /// <summary>
        /// Sorts the elements in this list. Uses Array.Sort with the provided comparer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(IComparer<T> comparer)
            => Sort(0, Count, comparer);

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
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            Checks.IsTrue(index >= 0, "'index' must be non-negative number");
            Checks.IsTrue(count >= 0, "'count' must be non-negative number");
            Checks.IsTrue(_count - index >= count, "Invalid offset length");

            if (count > 1)
            {
                Array.Sort<T>(_buffer, index, count, comparer);
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

        public static StatelessList<TState, T> Prefill<TDerived>([NotNull] TState state, int count)
            where TDerived : T, new()
        {
            var list = new StatelessList<TState, T>(state, count);

            if (default(TDerived) == null)
            {
                var buffer = list._buffer;

                for (var i = 0; i < count; i++)
                {
                    buffer[i] = new TDerived();
                }
            }

            list._count = count;

            return list;
        }

        public static StatelessList<TState, T> Prefill([NotNull] TState state, T value, int count)
        {
            var list = new StatelessList<TState, T>(state, count);

            var buffer = list._buffer.AsSpan();

            for (var i = 0; i < count; i++)
            {
                buffer[i] = value;
            }

            list._count = count;

            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StatelessList<TState, T> PrefillNoInit([NotNull] TState state, int count)
            => new(state, count) { _count = count };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => new Internals.FasterListEnumerator<T>(_buffer, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => new Internals.FasterListEnumerator<T>(_buffer, _count);
    }
}
