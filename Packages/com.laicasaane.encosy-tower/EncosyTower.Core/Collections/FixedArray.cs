#if UNITY_BURST

// <copyright file="FixedArray.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace EncosyTower.Collections
{
    /// <summary>
    /// Represents a fixed-size array.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <typeparam name="TBuffer">
    /// <para>The type of the buffer blob to store elements of <typeparamref name="T"/>.</para>
    /// <para>The size of <typeparamref name="TBuffer"/> (in bytes) must be greater than
    /// or equal to the size of <typeparamref name="T"/>.</para>
    /// </typeparam>
    public unsafe readonly struct FixedArray<T, TBuffer> : IReadOnlyList<T>
        , IAsSpan<T>, IAsReadOnlySpan<T>
        , ICopyToSpan<T>, ITryCopyToSpan<T>
        , ICopyFromSpan<T>, ITryCopyFromSpan<T>
        where T : unmanaged
        where TBuffer : unmanaged
    {
        private readonly TBuffer _buffer;

        static FixedArray()
        {
            ThrowIfInvalidSize();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedArray()
        {
            _buffer = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedArray(TBuffer data)
        {
            _buffer = data;
        }

        /// <summary>
        /// The number of elements of <typeparamref name="T"/> contained
        /// in the <see cref="FixedArray{T, TBuffer}"/>.
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnsafeUtility.SizeOf<TBuffer>() / UnsafeUtility.SizeOf<T>();
        }

        /// <summary>
        /// The size of <typeparamref name="T"/> in bytes multiplied by <see cref="Length"/>.
        /// </summary>
        public int LengthInBytes
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Length * UnsafeUtility.SizeOf<T>();
        }

        /// <summary>
        /// The size of <typeparamref name="TBuffer"/> in bytes.
        /// </summary>
        public int CapacityInBytes
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnsafeUtility.SizeOf<TBuffer>();
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Checks.IndexInRange(index, Length);
                return UnsafeUtility.ReadArrayElement<T>(Buffer, Checks.BurstAssumePositive(index));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Checks.IndexInRange(index, Length);
                UnsafeUtility.WriteArrayElement(Buffer, Checks.BurstAssumePositive(index), value);
            }
        }

        private T* Buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                fixed (void* ptr = &_buffer)
                {
                    return (T*)ptr;
                }
            }
        }

        int IReadOnlyCollection<T>.Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(in FixedArray<T, TBuffer> array)
            => array.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(in FixedArray<T, TBuffer> array)
            => array.AsReadOnlySpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ElementAt(int index)
        {
            Checks.IndexInRange(index, Length);
            return ref UnsafeUtility.ArrayElementAsRef<T>(Buffer, Checks.BurstAssumePositive(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] destination, int destinationIndex)
            => CopyTo(destination.AsSpan().Slice(destinationIndex, Length));

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
            => source[..length].CopyTo(AsSpan().Slice(destinationStartIndex, length));

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
            => source[..length].TryCopyTo(AsSpan().Slice(destinationStartIndex, length));

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
            => AsReadOnlySpan().Slice(sourceStartIndex, length).CopyTo(destination[..length]);

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
            => AsReadOnlySpan().Slice(sourceStartIndex, length).TryCopyTo(destination[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly()
            => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
            => new(Buffer, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
            => new(Buffer, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
            => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        private static void ThrowIfInvalidSize()
        {
            if (UnsafeUtility.SizeOf<TBuffer>() < UnsafeUtility.SizeOf<T>())
            {
                throw new InvalidOperationException(
                    $"sizeof({typeof(TBuffer)}) must be greater than or equal to sizeof({typeof(T)})"
                );
            }
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly FixedArray<T, TBuffer> _array;
            private int _index;
            private T _current;

            public Enumerator(in FixedArray<T, TBuffer> array)
            {
                _array = array;
                _index = 0;
                _current = default;
            }

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (((uint)_index < (uint)_array.Length))
                {
                    _current = _array[_index];
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                _index = _array.Length + 1;
                _current = default;
                return false;
            }

            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }

            public void Reset()
            {
                _index = 0;
                _current = default;
            }

            readonly object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _array.Length + 1)
                    {
                        ThrowHelper.ThrowInvalidOperationException_EnumOpCantHappen();
                    }

                    return Current;
                }
            }
        }

        public readonly struct ReadOnly : IReadOnlyList<T>, IAsReadOnlySpan<T>, ICopyToSpan<T>, ITryCopyToSpan<T>
        {
            private readonly FixedArray<T, TBuffer> _array;

            static ReadOnly()
            {
                ThrowIfInvalidSize();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly(in FixedArray<T, TBuffer> array)
            {
                _array = array;
            }

            /// <summary>
            /// The number of elements of <typeparamref name="T"/> contained
            /// in the <see cref="FixedArray{T, TBuffer}"/>.
            /// </summary>
            public int Length
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _array.Length;
            }

            /// <summary>
            /// The size of <typeparamref name="T"/> in bytes multiplied by <see cref="Length"/>.
            /// </summary>
            public int LengthInBytes
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _array.LengthInBytes;
            }

            /// <summary>
            /// The size of <typeparamref name="TBuffer"/> in bytes.
            /// </summary>
            public int CapacityInBytes
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _array.CapacityInBytes;
            }

            int IReadOnlyCollection<T>.Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Length;
            }

            public T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _array[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(in FixedArray<T, TBuffer> array)
                => array.AsReadOnly();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnlySpan<T>(in ReadOnly array)
                => array.AsReadOnlySpan();

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
                => AsReadOnlySpan().Slice(sourceStartIndex, length).CopyTo(destination[..length]);

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
                => AsReadOnlySpan().Slice(sourceStartIndex, length).TryCopyTo(destination[..length]);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<T> AsReadOnlySpan()
                => _array.AsReadOnlySpan();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator()
                => new(_array);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
                => GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }
    }
}

#endif
