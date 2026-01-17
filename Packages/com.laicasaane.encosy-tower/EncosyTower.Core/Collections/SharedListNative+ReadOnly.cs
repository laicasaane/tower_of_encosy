using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.Collections
{
    partial class SharedList<T, TNative>
    {
        partial struct ReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SharedListNative<TNative>.ReadOnly AsNative()
                => new(_buffer.Reinterpret<TNative>(), _count, _version);
        }
    }

    partial struct SharedListNative<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly()
            => new(
                  _buffer.AsReadOnly()
                , _count.AsReadOnly()
                , _version.AsReadOnly()
            );

        public readonly struct ReadOnly : IReadOnlyList<T>, IAsReadOnlySpan<T>, ICopyToSpan<T>, ITryCopyToSpan<T>
            , IHasCapacity, IHasCount, IIsCreated
        {
            internal readonly NativeArray<T>.ReadOnly _buffer;
            internal readonly NativeArray<int>.ReadOnly _count;
            internal readonly NativeArray<int>.ReadOnly _version;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ReadOnly(
                  NativeArray<T>.ReadOnly buffer
                , NativeArray<int>.ReadOnly count
                , NativeArray<int>.ReadOnly version
            )
            {
                _buffer = buffer;
                _count = count;
                _version = version;
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
                    Checks.IsTrue(
                          (uint)index < (uint)Count
                        , "index is outside the range of valid indices for the SharedList<T>.ReadOnly"
                    );

                    return _buffer.AsReadOnlySpan()[index];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(in SharedListNative<T> list)
                => list.AsReadOnly();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnlySpan<T>(in ReadOnly list)
                => list.AsReadOnlySpan();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<T> AsReadOnlySpan()
                => _buffer.AsReadOnlySpan()[..Count];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(T[] destination, int destinationIndex)
                => CopyTo(destination.AsSpan().Slice(destinationIndex, Count));

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
            public T[] ToArray()
                => AsReadOnlySpan().ToArray();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SharedListNative<U>.ReadOnly Reinterpret<U>()
                where U : unmanaged
            {
                return new SharedListNative<U>.ReadOnly(
                      _buffer.Reinterpret<U>()
                    , _count
                    , _version
                );
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
                => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }
    }
}
