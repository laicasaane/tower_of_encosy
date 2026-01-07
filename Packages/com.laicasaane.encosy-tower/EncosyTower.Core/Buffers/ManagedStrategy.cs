// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/DualMemorySupport/ManagedStrategy.cs

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
    /// They are called strategy because they abstract the handling of the memory type used.
    /// Through the IBufferStrategy interface, external datastructure can use interchangeably native and managed memory.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ManagedStrategy<T> : IBufferStrategy<T>
    {
        internal MBInternal<T> _realBuffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ManagedStrategy(int size) : this()
        {
            Alloc(size);
        }

        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _realBuffer.Capacity;
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _realBuffer.IsCreated;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _realBuffer[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Alloc(int size)
        {
            var b =  default(MBInternal<T>);
            b.Set(new T[size]);
            _realBuffer = b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Alloc(int size, AllocatorStrategy allocator, bool memClear = true)
        {
            var b =  default(MBInternal<T>);
            var array = new T[size];
            b.Set(array);
            _realBuffer = b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize)
            => Resize(newSize, true, true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize, bool copyContent)
            => Resize(newSize, copyContent, true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize, bool copyContent, bool memClear)
        {
            if (newSize == Capacity)
            {
                return;
            }

            var realBuffer = _realBuffer.ToManagedArray();

            if (copyContent)
                Array.Resize(ref realBuffer, newSize);
            else
                realBuffer = new T[newSize];

            var b = default(MBInternal<T>);
            b.Set(realBuffer);
            _realBuffer = b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void FastClear()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                _realBuffer.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear()
            => _realBuffer.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly MB<T> ToRealBuffer()
            => _realBuffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ArraySegment<T> AsArraySegment()
            => _realBuffer.AsArraySegment();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan()
            => _realBuffer.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan()
            => _realBuffer.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnly AsReadOnly()
            => this;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() { }

        public struct ReadOnly : IReadOnlyBufferStrategy<T>
        {
            internal MBInternal<T>.ReadOnly _realBuffer;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ReadOnly(ManagedStrategy<T> strategy) : this()
            {
                _realBuffer = strategy._realBuffer;
            }

            public readonly int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _realBuffer.Capacity;
            }

            public readonly bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _realBuffer.IsCreated;
            }

            public ref readonly T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _realBuffer[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly MB<T>.ReadOnly ToRealBuffer()
                => _realBuffer;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly ReadOnlySpan<T> AsReadOnlySpan()
                => _realBuffer.AsReadOnlySpan();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(ManagedStrategy<T> strategy)
                => new(strategy);
        }
    }
}
