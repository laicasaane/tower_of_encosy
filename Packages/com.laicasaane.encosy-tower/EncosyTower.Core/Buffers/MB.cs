// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/DualMemorySupport/MB.cs

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
using System.Runtime.CompilerServices;

namespace EncosyTower.Buffers
{
    /// <summary>
    /// MB stands for ManagedBuffer
    /// <br/>
    /// MBs are note meant to be resized or freed. They are wrappers of constant size arrays.
    /// <br/>
    /// MBs always wrap external arrays, they are not meant to allocate memory by themselves.
    ///<br/>
    /// MB are wrappers of arrays. Are not meant to resize or free.
    /// <br/>
    /// MBs cannot have a count, because a count of the meaningful number of items is not tracked.
    /// For example, an MB could be initialized with a size 10 and count 0. Then the buffer is used
    /// to fill entities but the count will stay zero.
    /// <br/>
    /// It's not the MB responsibility to track the count.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public ref struct MB<T>
    {
        private MBInternal<T> _bufferImplementation;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MB(MBInternal<T> mbInternal)
        {
            _bufferImplementation = mbInternal;
        }

        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bufferImplementation.Capacity;
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bufferImplementation.IsCreated;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG && ENABLE_PARANOID_CHECKS
                if (index >= _buffer.Length)
                    throw new IndexOutOfRangeException("Paranoid check failed!");
#endif

                return ref _bufferImplementation[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(T[] array)
        {
            _bufferImplementation.Set(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear()
        {
            _bufferImplementation.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(ReadOnlySpan<T> source)
            => CopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(ReadOnlySpan<T> source, int length)
            => CopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => CopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
            => source[..length].CopyTo(AsSpan().Slice(destinationStartIndex, length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(ReadOnlySpan<T> source)
            => TryCopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(ReadOnlySpan<T> source, int length)
            => TryCopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => TryCopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
            => source[..length].TryCopyTo(AsSpan().Slice(destinationStartIndex, length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> destination)
            => CopyTo(0, destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> destination, int length)
            => CopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(int sourceStartIndex, Span<T> destination)
            => CopyTo(sourceStartIndex, destination, destination.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(int sourceStartIndex, Span<T> destination, int length)
            => AsReadOnlySpan().Slice(sourceStartIndex, length).CopyTo(destination[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> destination)
            => TryCopyTo(0, destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> destination, int length)
            => TryCopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination)
            => TryCopyTo(sourceStartIndex, destination, destination.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
            => AsReadOnlySpan().Slice(sourceStartIndex, length).TryCopyTo(destination[..length]);

        /// <summary>
        /// todo: this must go away, it's not safe. it must become internal and only used by the framework
        /// externally should use the AsReader, AsWriter, AsReadOnly, AsParallelReader, AsParallelWriter pattern
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly T[] ToManagedArray()
        {
            return _bufferImplementation.ToManagedArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan()
        {
            return _bufferImplementation.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan()
        {
            return _bufferImplementation.AsSpan();
        }
    }

    internal struct MBInternal<T> : IBuffer<T>
    {
        private T[] _buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        MBInternal(T[] array)
        {
            _buffer = array;
        }

        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Length;
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer != null;
        }

        public readonly ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG && ENABLE_PARANOID_CHECKS
                if (index >= _buffer.Length)
                    throw new IndexOutOfRangeException("Paranoid check failed!");
#endif

                return ref _buffer[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(T[] array)
        {
            _buffer = array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(ReadOnlySpan<T> source)
            => CopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(ReadOnlySpan<T> source, int length)
            => CopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => CopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
            => source[..length].CopyTo(AsSpan().Slice(destinationStartIndex, length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(ReadOnlySpan<T> source)
            => TryCopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(ReadOnlySpan<T> source, int length)
            => TryCopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => TryCopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
            => source[..length].TryCopyTo(AsSpan().Slice(destinationStartIndex, length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> destination)
            => CopyTo(0, destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> destination, int length)
            => CopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(int sourceStartIndex, Span<T> destination)
            => CopyTo(sourceStartIndex, destination, destination.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(int sourceStartIndex, Span<T> destination, int length)
            => AsReadOnlySpan().Slice(sourceStartIndex, length).CopyTo(destination[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> destination)
            => TryCopyTo(0, destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> destination, int length)
            => TryCopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination)
            => TryCopyTo(sourceStartIndex, destination, destination.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
            => AsReadOnlySpan().Slice(sourceStartIndex, length).TryCopyTo(destination[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly T[] ToManagedArray()
        {
            return _buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ArraySegment<T> AsArraySegment()
        {
            return new ArraySegment<T>(_buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan()
        {
            return _buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan()
        {
            return _buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MB<T>(MBInternal<T> proxy)
            => new(proxy);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MBInternal<T>(MB<T> proxy)
            => new(proxy.ToManagedArray());
    }
}
