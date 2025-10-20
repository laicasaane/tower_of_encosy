using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections
{
    partial struct SharedListNative<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly()
            => new(this);

        public readonly struct ReadOnly : IAsReadOnlySpan<T>, IReadOnlyList<T>
        {
            internal readonly NativeArray<T>.ReadOnly _buffer;
            internal readonly NativeArray<int>.ReadOnly _count;
            internal readonly NativeArray<int>.ReadOnly _version;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly(SharedListNative<T> list)
            {
                _buffer = list._buffer.AsReadOnly();
                _count = list._count.AsReadOnly();
                _version = list._version.AsReadOnly();
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _buffer.IsCreated && _count.IsCreated && _version.IsCreated;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _count[0];
            }

            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _buffer.Length;
            }

            public bool IsReadOnly => true;

            internal int Version
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _version[0];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator()
                => new(this);

            public T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    Checks.IsTrue((uint)index < (uint)Count, "index is outside the range of valid indices for the SharedList<T>.ReadOnly");
                    return _buffer.AsReadOnlySpan()[index];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(in SharedListNative<T> list)
                => list.AsReadOnly();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<T> AsReadOnlySpan()
                => _buffer.AsReadOnlySpan()[..Count];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(T[] array)
                => CopyTo(array, 0);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(T[] array, int arrayIndex)
                => CopyTo(0, array, arrayIndex, Count);

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
            public T[] ToArray()
                => AsReadOnlySpan().ToArray();

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
                => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }
    }
}
