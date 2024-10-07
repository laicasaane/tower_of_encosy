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

namespace Module.Core.Buffers
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

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bufferImplementation.Capacity;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bufferImplementation.IsValid;
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
        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count)
        {
            _bufferImplementation.CopyTo(sourceStartIndex, destination, destinationStartIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _bufferImplementation.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(T[] array)
        {
            _bufferImplementation.Set(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(T[] collection, uint actualSize)
        {
            _bufferImplementation.CopyFrom(collection, actualSize);
        }

        /// <summary>
        /// todo: this must go away, it's not safe. it must become internal and only used by the framework
        /// externally should use the AsReader, AsWriter, AsReadOnly, AsParallelReader, AsParallelWriter pattern
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToManagedArray()
        {
            return _bufferImplementation.ToManagedArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            return _bufferImplementation.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
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

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Length;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer != null;
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

                return ref _buffer[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(T[] array)
        {
            _buffer = array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(T[] collection, uint actualSize)
        {
            Array.Copy(collection, 0, _buffer, 0, actualSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count)
        {
            Array.Copy(_buffer, sourceStartIndex, destination, destinationStartIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToManagedArray()
        {
            return _buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            return _buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
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
