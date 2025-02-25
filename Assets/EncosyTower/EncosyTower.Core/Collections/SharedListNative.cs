using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections
{
    public readonly struct SharedListNative<T, TNative>
        : IAsSpan<TNative>, IAsReadOnlySpan<TNative>, IAsNativeSlice<TNative>
        , IClearable
        where T : unmanaged
        where TNative : unmanaged
    {
        internal readonly NativeArray<TNative> _buffer;
        internal readonly NativeArray<int> _count;
        internal readonly NativeArray<int> _version;

        public SharedListNative([NotNull] SharedList<T, TNative> list)
        {
            _buffer = list._buffer.AsNativeArray();
            _count = list._count.AsNativeArray();
            _version = list._version.AsNativeArray();
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.IsCreated;
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

        public TNative this[int index]
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
        public void Add(TNative item)
        {
            VersionRW++;

            ref var count = ref CountRW;

            Checks.IsTrue(count < _buffer.Length, "the capacity of SharedListNative<T> is immutable and cannot change");

            _buffer.AsSpan()[count++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in TNative item)
        {
            VersionRW++;

            ref var count = ref CountRW;

            Checks.IsTrue(count < _buffer.Length, "the capacity of SharedListNative<T> is immutable and cannot change");

            _buffer.AsSpan()[count++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, TNative item)
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
        public void Insert(int index, in TNative item)
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
        public ref TNative ElementAt(int index)
        {
            Checks.IsTrue((uint)index < (uint)Count, "index is outside the range of valid indexes for the SharedListNative<T>");
            return ref _buffer.AsSpan()[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(ReadOnlySpan<TNative> items)
            => AddRange(items, items.Length);

        public void AddRange(ReadOnlySpan<TNative> items, int count)
        {
            VersionRW++;

            if (count == 0) return;

            Checks.IsTrue(_buffer.Length - Count >= count, "the capacity of SharedListNative<T> is immutable and cannot change");

            items[..count].CopyTo(_buffer.AsSpan().Slice(Count, count));
            CountRW += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(NativeArray<TNative> items)
            => AddRange(items, items.Length);

        public void AddRange(NativeArray<TNative> items, int count)
        {
            VersionRW++;

            if (count == 0) return;

            Checks.IsTrue(_buffer.Length - Count >= count, "the capacity of SharedListNative<T> is immutable and cannot change");

            items.AsSpan()[..count].CopyTo(_buffer.AsSpan().Slice(Count, count));
            CountRW += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(NativeSlice<TNative> items)
            => AddRange(items, items.Length);

        public void AddRange(NativeSlice<TNative> items, int count)
        {
            VersionRW++;

            if (count == 0) return;

            Checks.IsTrue(_buffer.Length - Count >= count, "the capacity of SharedListNative<T> is immutable and cannot change");

            items.AsReadOnlySpan()[..count].CopyTo(_buffer.AsSpan().Slice(Count, count));
            CountRW += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<TNative> array)
            => CopyTo(0, array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, Span<TNative> array)
            => CopyTo(index, array, array.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, Span<TNative> array, int length)
            => AsReadOnlySpan().Slice(index, length).CopyTo(array[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(NativeArray<TNative> array)
            => CopyTo(0, array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, NativeArray<TNative> array)
            => CopyTo(index, array, array.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, NativeArray<TNative> array, int length)
            => AsReadOnlySpan().Slice(index, length).CopyTo(array.AsSpan()[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, NativeSlice<TNative> array)
            => CopyTo(index, array, array.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, NativeSlice<TNative> array, int length)
            => AsReadOnlySpan().Slice(index, length).CopyTo(array.AsSpan()[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            VersionRW++;
            CountRW = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TNative>.Enumerator GetEnumerator()
            => AsReadOnlySpan().GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TNative Peek()
            => ref _buffer.AsReadOnlySpan()[Count - 1];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TNative Pop()
        {
            VersionRW++;
            --CountRW;
            return ref _buffer.AsReadOnlySpan()[Count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Push(TNative item)
        {
            Insert(Count, item);
            return Count - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Push(in TNative item)
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

        public void RemoveAt(int startIndex, int length)
        {
            VersionRW++;

            Checks.IsTrue((uint)startIndex < (uint)Count, "out of bound start index");

            var end = startIndex + length;

            Checks.IsTrue((uint)end <= (uint)Count, "out of bound length");

            CountRW -= length;

            if (end == Count)
            {
                return;
            }

            _buffer.MemoryCopyUnsafe(end, startIndex, length);
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
        public TNative[] ToArray()
        {
            return AsReadOnlySpan().ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TNative> AsSpan()
        {
            VersionRW++;
            return _buffer.AsSpan()[..Count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TNative> AsReadOnlySpan()
        {
            return _buffer.AsReadOnlySpan()[..Count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSlice<TNative> AsNativeSlice()
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

        public void AddReplicate(TNative value, int amount)
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
        public static SharedListNative<T, TNative> Prefill(int amount)
        {
            SharedListNative<T, TNative> list = new SharedList<T, TNative>(amount);
            list.AddReplicate(amount);
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SharedListNative<T, TNative> Prefill(TNative value, int amount)
        {
            SharedListNative<T, TNative> list = new SharedList<T, TNative>(amount);
            list.AddReplicate(value, amount);
            return list;
        }
    }
}
