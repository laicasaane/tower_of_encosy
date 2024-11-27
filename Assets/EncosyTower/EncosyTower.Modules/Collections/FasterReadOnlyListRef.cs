// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/Arrays/LocalFasterReadOnlyList.cs

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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Collections
{
    public readonly ref struct FasterReadOnlyListRef<T>
    {
        private readonly T[] _buffer;
        private readonly int _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterReadOnlyListRef([NotNull] FasterList<T> list)
        {
            _buffer = list._buffer;
            _count = list._count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterReadOnlyListRef(FasterReadOnlyList<T> list)
        {
            _buffer = list._list._buffer;
            _count = list._list._count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterReadOnlyListRef([NotNull] T[] list, int count)
        {
            _buffer = list;
            _count = count;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator FasterReadOnlyListRef<T>(FasterList<T> list)
            => new(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
            => new(_buffer, Count);

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
            => _buffer.AsSpan(_count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
            => _buffer.AsSpan(_count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory()
            => _buffer.AsMemory(_count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> AsReadOnlyMemory()
            => _buffer.AsMemory(_count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            var array = new T[_count];
            Array.Copy(_buffer, 0, array, 0, _count);

            return array;
        }

        public struct Enumerator
        {
            private readonly T[] _list;
            private readonly int _count;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(T[] list, int count)
            {
                _list = list;
                _count = count;
                _index = -1;
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _list[_index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
                => ++_index < _count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }
        }
    }
}