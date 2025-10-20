using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections
{
    partial class SharedList<T, TNative>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SharedListNative<TNative> AsNative()
            => new(
                  _buffer.AsNativeArray()
                , _count.AsNativeArray()
                , _version.AsNativeArray()
            );
    }

    public readonly partial struct SharedListNative<T>
        : IReadOnlyList<T>
        , IAsSpan<T>, IAsReadOnlySpan<T>, IAsNativeSlice<T>
        , IAddRangeSpan<T>
        , IClearable
        where T : unmanaged
    {
        internal readonly NativeArray<T> _buffer;
        internal readonly NativeArray<int> _count;
        internal readonly NativeArray<int> _version;

        internal SharedListNative(
              NativeArray<T> buffer
            , NativeArray<int> count
            , NativeArray<int> version
        )
        {
            _buffer = buffer;
            _count = count;
            _version = version;
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.IsCreated && _count.IsCreated && _version.IsCreated;
        }

        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Length;
        }

        public readonly int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count.AsReadOnlySpan()[0];
        }

        internal readonly ref int CountRW
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _count.AsSpan()[0];
        }

        internal readonly ref int VersionRW
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _version.AsSpan()[0];
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                Checks.IsTrue((uint)index < (uint)Count, "index is outside the range of valid indexes for the SharedListNative<T>");
                return _buffer.AsReadOnlySpan()[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Checks.IsTrue((uint)index < (uint)Count, "index is outside the range of valid indexes for the SharedListNative<T>");
                VersionRW++;
                _buffer.AsSpan()[index] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            VersionRW++;

            ref var count = ref CountRW;

            Checks.IsTrue(count < _buffer.Length, "the capacity of SharedListNative<T> is immutable and cannot change");

            _buffer.AsSpan()[count++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item)
        {
            VersionRW++;

            ref var count = ref CountRW;

            Checks.IsTrue(count < _buffer.Length, "the capacity of SharedListNative<T> is immutable and cannot change");

            _buffer.AsSpan()[count++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, T item)
        {
            VersionRW++;

            ref var count = ref CountRW;

            Checks.IsTrue((uint)index < (uint)count, "index is outside the range of valid indexes for the SharedListNative<T>");
            Checks.IsTrue(count < _buffer.Length, "the capacity of SharedListNative<T> is immutable and cannot change");

            _buffer.MemoryCopyUnsafe(index, index + 1, count - index);
            ++count;

            _buffer.AsSpan()[index] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in T item)
        {
            VersionRW++;

            ref var count = ref CountRW;

            Checks.IsTrue((uint)index < (uint)count, "index is outside the range of valid indexes for the SharedListNative<T>");
            Checks.IsTrue(count < _buffer.Length, "the capacity of SharedListNative<T> is immutable and cannot change");

            _buffer.MemoryCopyUnsafe(index, index + 1, count - index);
            ++count;

            _buffer.AsSpan()[index] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ElementAt(int index)
        {
            Checks.IsTrue((uint)index < (uint)Count, "index is outside the range of valid indexes for the SharedListNative<T>");
            return ref _buffer.AsSpan()[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(ReadOnlySpan<T> items)
            => AddRange(items, items.Length);

        public void AddRange(ReadOnlySpan<T> items, int count)
        {
            VersionRW++;

            if (count == 0) return;

            Checks.IsTrue(_buffer.Length - Count >= count, "the capacity of SharedListNative<T> is immutable and cannot change");

            items[..count].CopyTo(_buffer.AsSpan().Slice(Count, count));
            CountRW += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(NativeArray<T> items)
            => AddRange(items, items.Length);

        public void AddRange(NativeArray<T> items, int count)
        {
            VersionRW++;

            if (count == 0) return;

            Checks.IsTrue(_buffer.Length - Count >= count, "the capacity of SharedListNative<T> is immutable and cannot change");

            items.AsSpan()[..count].CopyTo(_buffer.AsSpan().Slice(Count, count));
            CountRW += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(NativeSlice<T> items)
            => AddRange(items, items.Length);

        public void AddRange(NativeSlice<T> items, int count)
        {
            VersionRW++;

            if (count == 0) return;

            Checks.IsTrue(_buffer.Length - Count >= count, "the capacity of SharedListNative<T> is immutable and cannot change");

            var span = _buffer.AsSpan().Slice(Count, count);
            var buffer = NativeArrayUnsafe.ConvertFrom(span, Allocator.None);
            items.Slice(0, count).CopyTo(buffer);

            CountRW += count;
        }

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
        public void CopyTo(NativeArray<T> array)
            => CopyTo(0, array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, NativeArray<T> array)
            => CopyTo(index, array, array.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, NativeArray<T> array, int length)
            => AsReadOnlySpan().Slice(index, length).CopyTo(array.AsSpan()[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, NativeSlice<T> array)
            => CopyTo(index, array, array.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, NativeSlice<T> array, int length)
            => array.Slice(0, length).CopyFrom(AsNativeSlice().Slice(index, length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            VersionRW++;
            CountRW = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
            => new(AsReadOnly());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Peek()
            => ref _buffer.AsReadOnlySpan()[Count - 1];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Pop()
        {
            VersionRW++;
            --CountRW;
            return ref _buffer.AsReadOnlySpan()[Count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Push(T item)
        {
            Insert(Count, item);
            return Count - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Push(in T item)
        {
            Insert(Count, item);
            return Count - 1;
        }

        public void RemoveAt(int index)
        {
            VersionRW++;

            Checks.IsTrue((uint)index < (uint)Count, "out of bound index");

            if (index < --CountRW)
            {
                _buffer.MemoryCopyUnsafe(index + 1, index, Count - index);
            }
        }

        public void RemoveRange(int startIndex, int length)
        {
            VersionRW++;

            var count = Count;

            Checks.IsTrue((uint)startIndex < (uint)count, "out of bound start index");

            var end = startIndex + length;

            Checks.IsTrue((uint)end <= (uint)count, "out of bound length");

            if (length < 1)
            {
                return;
            }

            count = CountRW -= length;
            _buffer.MemoryCopyUnsafe(end, startIndex, count);
        }

        public void RemoveAtSwapBack(int index)
        {
            VersionRW++;

            Checks.IsTrue((uint)index < (uint)Count, "out of bound index");

            if (index < --CountRW)
            {
                var buffer = _buffer.AsSpan();
                buffer[index] = buffer[Count];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            return AsReadOnlySpan().ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            VersionRW++;
            return _buffer.AsSpan()[..Count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            return _buffer.AsReadOnlySpan()[..Count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSlice<T> AsNativeSlice()
        {
            return _buffer.Slice(0, Count);
        }

        public void AddReplicate(int amount)
        {
            VersionRW++;

            var oldCount = Count;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Length;

            Checks.IsTrue(offset < 1, "the capacity of SharedListNative<T> is immutable and cannot change");

            _buffer.AsSpan().Slice(oldCount, amount).Fill(default);
            CountRW = newCount;
        }

        public void AddReplicate(T value, int amount)
        {
            VersionRW++;

            var oldCount = Count;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Length;

            Checks.IsTrue(offset < 1, "the capacity of SharedListNative<T> is immutable and cannot change");

            _buffer.AsSpan().Slice(oldCount, amount).Fill(value);
            CountRW = newCount;
        }

        public void AddReplicateNoInit(int amount)
        {
            VersionRW++;

            var oldCount = Count;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Length;

            Checks.IsTrue(offset < 1, "the capacity of SharedListNative<T> is immutable and cannot change");

            CountRW = newCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SharedListNative<T> Prefill(int amount)
        {
            SharedListNative<T> list = new SharedList<T, T>(amount);
            list.AddReplicate(amount);
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SharedListNative<T> Prefill(T value, int amount)
        {
            SharedListNative<T> list = new SharedList<T, T>(amount);
            list.AddReplicate(value, amount);
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
