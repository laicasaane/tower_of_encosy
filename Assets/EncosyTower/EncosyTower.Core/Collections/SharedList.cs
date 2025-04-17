using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections
{
    public class SharedList<T> : SharedList<T, T>
        where T : unmanaged
    {
        public SharedList() : base()
        {
        }

        public SharedList(int capacity) : base(capacity)
        {
        }

        public SharedList([NotNull] params T[] source) : base(source)
        {
        }

        public SharedList(in ArraySegment<T> source) : base(source)
        {
        }

        public SharedList(in ReadOnlySpan<T> source) : base(source)
        {
        }

        public SharedList(in NativeArray<T> source) : base(source)
        {
        }

        public SharedList(in NativeSlice<T> source) : base(source)
        {
        }

        public SharedList([NotNull] ICollection<T> source) : base(source)
        {
        }

        public SharedList([NotNull] ICollection<T> source, int extraSize) : base(source, extraSize)
        {
        }

        public SharedList([NotNull] in SharedList<T, T> source) : base(source)
        {
        }
    }

    public class SharedList<T, TNative> : IList<T>, IReadOnlyList<T>
        , IAsSpan<T>, IAsReadOnlySpan<T>
        , IAsMemory<T>, IAsReadOnlyMemory<T>
        , IClearable, IDisposable
        where T : unmanaged
        where TNative : unmanaged
    {
        internal readonly SharedArray<T, TNative> _buffer;
        internal readonly SharedReference<int> _count;
        internal readonly SharedReference<int> _version;

        public SharedList()
        {
            _buffer = new(0);
            _count = new(0);
            _version = new(0);
        }

        public SharedList(int capacity)
        {
            _buffer = new(capacity);
            _count = new(0);
            _version = new(0);
        }

        public SharedList([NotNull] params T[] source)
        {
            _buffer = new(source);
            _count = new(source.Length);
            _version = new(0);
        }

        public SharedList(in ArraySegment<T> source)
        {
            _buffer = new(source);
            _count = new(source.Count);
            _version = new(0);
        }

        public SharedList(in ReadOnlySpan<T> source)
        {
            _buffer = new(source);
            _count = new(source.Length);
            _version = new(0);
        }

        public SharedList([NotNull] ICollection<T> source)
        {
            _buffer = new(source);
            _count = new(source.Count);
            _version = new(0);
        }

        public SharedList([NotNull] ICollection<T> source, int extraSize)
        {
            _buffer = new(source, extraSize);
            _count = new(source.Count);
            _version = new(0);
        }

        public SharedList(in NativeArray<TNative> source)
        {
            _buffer = new(source);
            _count = new(source.Length);
            _version = new(0);
        }

        public SharedList(in NativeSlice<TNative> source)
        {
            _buffer = new(source);
            _count = new(source.Length);
            _version = new(0);
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count.ValueRO;
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
                Checks.IsTrue((uint)index < (uint)_count.ValueRO, "index is outside the range of valid indices for the SharedList<T>");
                return _buffer.AsReadOnlySpan()[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Checks.IsTrue((uint)index < (uint)_count.ValueRO, "index is outside the range of valid indices for the SharedList<T>");
                _version.ValueRW++;
                _buffer.AsSpan()[index] = value;
            }
        }

        public void Dispose()
        {
            _buffer.Dispose();
            _count.Dispose();
            _version.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            _version.ValueRW++;

            ref var count = ref _count.ValueRW;

            if (count == _buffer.Length)
                AllocateMore();

            _buffer.AsSpan()[count++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item)
        {
            _version.ValueRW++;

            ref var count = ref _count.ValueRW;

            if (count == _buffer.Length)
                AllocateMore();

            _buffer.AsSpan()[count++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, T item)
        {
            _version.ValueRW++;

            ref var count = ref _count.ValueRW;

            Checks.IsTrue((uint)index < (uint)count, "index is outside the range of valid indices for the SharedList<T>");

            if (count == _buffer.Length)
                AllocateMore();

            var buffer = _buffer.AsManagedArray();
            Array.Copy(buffer, index, buffer, index + 1, count - index);
            ++count;

            _buffer.AsSpan()[index] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in T item)
        {
            _version.ValueRW++;

            ref var count = ref _count.ValueRW;

            Checks.IsTrue((uint)index < (uint)count, "index is outside the range of valid indices for the SharedList<T>");

            if (count == _buffer.Length)
                AllocateMore();

            var buffer = _buffer.AsManagedArray();
            Array.Copy(buffer, index, buffer, index + 1, count - index);
            ++count;

            _buffer.AsSpan()[index] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ElementAt(int index)
        {
            Checks.IsTrue((uint)index < (uint)_count.ValueRO, "index is outside the range of valid indices for the SharedList<T>");
            return ref _buffer.AsSpan()[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange([NotNull] T[] items)
            => AddRange(items, items.Length);

        public void AddRange([NotNull] T[] items, int count)
        {
            _version.ValueRW++;

            if (count == 0) return;

            if (_buffer.Length - _count.ValueRO < count)
                AllocateMore(checked(_count.ValueRO + count));

            items.AsSpan().CopyTo(_buffer.AsSpan().Slice(_count.ValueRO, count));
            _count.ValueRW += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(ReadOnlySpan<T> items)
            => AddRange(items, items.Length);

        public void AddRange(ReadOnlySpan<T> items, int count)
        {
            _version.ValueRW++;

            if (count == 0) return;

            if (_buffer.Length - _count.ValueRO < count)
                AllocateMore(checked(_count.ValueRO + count));

            items[..count].CopyTo(_buffer.AsSpan().Slice(_count.ValueRO, count));
            _count.ValueRW += count;
        }

        public void AddRange([NotNull] IEnumerable<T> collection)
        {
            if (collection is ICollection<T> c)
            {
                var count = c.Count;

                if (count > 0)
                {
                    if (_buffer.Length - _count.ValueRO < count)
                    {
                        AllocateMore(checked(_count.ValueRO + count));
                    }

                    c.CopyTo(_buffer.AsManagedArray(), _count.ValueRO);
                    _count.ValueRW += count;
                    _version.ValueRW++;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array)
            => CopyTo(array, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
            => CopyTo(0, array, arrayIndex, _count.ValueRO);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, T[] array, int arrayIndex, int length)
            => AsReadOnlySpan().Slice(index, length).CopyTo(array.AsSpan(arrayIndex, length));

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
            _version.ValueRW++;
            _count.ValueRW = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T>.Enumerator GetEnumerator()
            => AsReadOnlySpan().GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncreaseCapacityBy(int increment)
            => IncreaseCapacityTo(_buffer.Length + increment);

        public void IncreaseCapacityTo(int newCapacity)
        {
            _version.ValueRW++;

            if (newCapacity <= _buffer.Length)
            {
                return;
            }

            _buffer.Resize(newCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Peek()
            => ref _buffer.AsReadOnlySpan()[_count.ValueRO - 1];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Pop()
        {
            _version.ValueRW++;
            --_count.ValueRW;
            return ref _buffer.AsReadOnlySpan()[_count.ValueRO];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Push(T item)
        {
            Insert(_count.ValueRO, item);
            return _count.ValueRO - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Push(in T item)
        {
            Insert(_count.ValueRO, item);
            return _count.ValueRO - 1;
        }

        public void RemoveAt(int index)
        {
            _version.ValueRW++;

            Checks.IsTrue((uint)index < (uint)_count.ValueRO, "out of bound index");

            if (index < --_count.ValueRW)
            {
                var buffer = _buffer.AsManagedArray();
                Array.Copy(buffer, index + 1, buffer, index, _count.ValueRO - index);
            }
        }

        public void RemoveRange(int startIndex, int length)
        {
            _version.ValueRW++;

            var count = _count.ValueRO;

            Checks.IsTrue((uint)startIndex < (uint)count, "out of bound start index");

            var end = startIndex + length;

            Checks.IsTrue((uint)end <= (uint)count, "out of bound length");

            if (length < 1)
            {
                return;
            }

            count = _count.ValueRW -= length;

            var buffer = _buffer.AsManagedArray();

            Array.Copy(buffer, startIndex + length, buffer, startIndex, count - startIndex);
        }

        public void RemoveAtSwapBack(int index)
        {
            _version.ValueRW++;

            Checks.IsTrue((uint)index < (uint)_count.ValueRO, "out of bound index");

            if (index < --_count.ValueRW)
            {
                var buffer = _buffer.AsManagedArray();
                buffer[index] = buffer[_count.ValueRO];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize)
        {
            _version.ValueRW++;

            if (newSize == _buffer.Length)
            {
                return;
            }

            _buffer.Resize(newSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            return AsReadOnlySpan().ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            _version.ValueRW++;
            return _buffer.AsSpan()[.._count.ValueRO];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            return _buffer.AsReadOnlySpan()[.._count.ValueRO];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory()
        {
            _version.ValueRW++;
            return _buffer.AsMemory()[.._count.ValueRO];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> AsReadOnlyMemory()
        {
            return _buffer.AsReadOnlyMemory()[.._count.ValueRO];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            if (_count.ValueRO < _buffer.Length)
                Resize(_count.ValueRO);
        }

        public void AddReplicate(int amount)
        {
            _version.ValueRW++;

            var oldCount = _count.ValueRO;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Length;

            if (offset > 0)
            {
                AllocateMore(newCount);
            }

            _buffer.AsSpan().Slice(oldCount, amount).Fill(default);
            _count.ValueRW = newCount;
        }

        public void AddReplicate(T value, int amount)
        {
            _version.ValueRW++;

            var oldCount = _count.ValueRO;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Length;

            if (offset > 0)
            {
                AllocateMore(newCount);
            }

            _buffer.AsSpan().Slice(oldCount, amount).Fill(value);
            _count.ValueRW = newCount;
        }

        public void AddReplicateNoInit(int amount)
        {
            _version.ValueRW++;

            var oldCount = _count.ValueRO;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Length;

            if (offset > 0)
            {
                AllocateMore(newCount);
            }

            _count.ValueRW = newCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SharedList<T, TNative> Prefill(int amount)
        {
            var list = new SharedList<T, TNative>(amount);
            list.AddReplicate(amount);
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SharedList<T, TNative> Prefill(T value, int amount)
        {
            var list = new SharedList<T, TNative>(amount);
            list.AddReplicate(value, amount);
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SharedListNative<T, TNative>(SharedList<T, TNative> list)
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
            _buffer.Resize(newCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AllocateMore(int newSize)
        {
            Checks.IsTrue(newSize > _buffer.Length, "newSize is not greater than the current capacity");

            var newCapacity = CalcNewCapacity(newSize);
            _buffer.Resize(newCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => new Internals.FasterListEnumerator<T>(_buffer.AsManagedArray(), _count.ValueRO);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => new Internals.FasterListEnumerator<T>(_buffer.AsManagedArray(), _count.ValueRO);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IList<T>.IndexOf(T item)
            => Array.IndexOf(_buffer.AsManagedArray(), item, 0, _count.ValueRO);

        bool ICollection<T>.Remove(T item)
            => throw new NotImplementedException("Use SharedListExtensions.Remove instead.");

        bool ICollection<T>.Contains(T item)
            => throw new NotImplementedException("Use SharedListExtensions.Contains instead.");
    }
}
