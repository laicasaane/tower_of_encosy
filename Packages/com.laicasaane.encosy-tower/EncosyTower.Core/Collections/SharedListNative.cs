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

    /// <remarks>
    /// <para>SharedListNative is not thread safe.</para>
    /// <para>The capacity of SharedListNative is immutable, cannot change.</para>
    /// </remarks>
    public readonly partial struct SharedListNative<T>
        : IReadOnlyList<T>
        , IAsSpan<T>, IAsReadOnlySpan<T>, IAsNativeSlice<T>
        , ICopyFromSpan<T>, ITryCopyFromSpan<T>
        , ICopyToSpan<T>, ITryCopyToSpan<T>
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
        public void AddRange(in NativeSlice<T> items)
            => AddRange(items, items.Length);

        public void AddRange(in NativeSlice<T> items, int count)
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
        public void CopyFrom(ReadOnlySpan<T> source)
            => CopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ReadOnlySpan<T> source, int length)
            => CopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => CopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
            => new CopyFromSpan<T>(AsSpan()).CopyFrom(destinationStartIndex, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(ReadOnlySpan<T> source)
            => TryCopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(ReadOnlySpan<T> source, int length)
            => TryCopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => TryCopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
            => new CopyFromSpan<T>(AsSpan()).TryCopyFrom(destinationStartIndex, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
            => CopyTo(array.AsSpan().Slice(arrayIndex, Count));

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
            => new CopyToSpan<T>(AsReadOnlySpan()).CopyTo(sourceStartIndex, destination, length);

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
            => new CopyToSpan<T>(AsReadOnlySpan()).TryCopyTo(sourceStartIndex, destination, length);

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
        public void CopyTo(int index, in NativeSlice<T> array)
            => CopyTo(index, array, array.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, in NativeSlice<T> array, int length)
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

        public Span<T> AddReplicate(int amount)
        {
            VersionRW++;

            var oldCount = Count;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Length;

            Checks.IsTrue(offset < 1, "the capacity of SharedListNative<T> is immutable and cannot change");

            var buffer = _buffer.AsSpan().Slice(oldCount, amount);
            buffer.Fill(default);
            CountRW = newCount;

            return buffer;
        }

        public Span<T> AddReplicate(T value, int amount)
        {
            VersionRW++;

            var oldCount = Count;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Length;

            Checks.IsTrue(offset < 1, "the capacity of SharedListNative<T> is immutable and cannot change");

            var buffer = _buffer.AsSpan().Slice(oldCount, amount);
            buffer.Fill(value);
            CountRW = newCount;

            return buffer;
        }

        public Span<T> AddReplicateNoInit(int amount)
        {
            VersionRW++;

            var oldCount = Count;
            var newCount = amount + oldCount;
            var offset = newCount - _buffer.Length;

            Checks.IsTrue(offset < 1, "the capacity of SharedListNative<T> is immutable and cannot change");

            var buffer = _buffer.AsSpan().Slice(oldCount, amount);
            CountRW = newCount;

            return buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SharedListNative<U> Reinterpret<U>()
            where U : unmanaged
        {
            return new SharedListNative<U>(
                  _buffer.Reinterpret<U>()
                , _count
                , _version
            );
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
