using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections
{
    partial struct SharedListNative<T, TNative>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly()
            => new(
                  _buffer.AsReadOnly()
                , _count.AsReadOnly()
                , _version.AsReadOnly()
            );

        public readonly struct ReadOnly : IAsReadOnlySpan<TNative>, IReadOnlyList<TNative>
        {
            internal readonly NativeArray<TNative>.ReadOnly _buffer;
            internal readonly NativeArray<int>.ReadOnly _count;
            internal readonly NativeArray<int>.ReadOnly _version;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly(
                  NativeArray<TNative>.ReadOnly buffer
                , NativeArray<int>.ReadOnly count
                , NativeArray<int>.ReadOnly version
            )
            {
                _buffer = buffer;
                _count = count;
                _version = version;
            }

            public bool IsValid
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

            public TNative this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    Checks.IsTrue((uint)index < (uint)Count, "index is outside the range of valid indices for the SharedList<T>.ReadOnly");
                    return _buffer.AsReadOnlySpan()[index];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(in SharedListNative<T, TNative> list)
                => list.AsReadOnly();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TNative> AsReadOnlySpan()
                => _buffer.AsReadOnlySpan()[..Count];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(TNative[] array)
                => CopyTo(array, 0);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(TNative[] array, int arrayIndex)
                => CopyTo(0, array, arrayIndex, Count);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int index, TNative[] array, int arrayIndex, int length)
                => AsReadOnlySpan().Slice(index, length).CopyTo(array.AsSpan(arrayIndex, length));

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
            public TNative[] ToArray()
                => AsReadOnlySpan().ToArray();

            IEnumerator<TNative> IEnumerable<TNative>.GetEnumerator()
                => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }
    }
}
