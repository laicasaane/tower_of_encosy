// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/Arrays/StatelessReadOnlyList.cs

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
using EncosyTower.Modules.Buffers;

namespace EncosyTower.Modules.Collections
{
    public readonly struct StatelessReadOnlyList<TState, T> : IAsSpan<T>, IAsReadOnlySpan<T>, IAsMemory<T>, IAsReadOnlyMemory<T>
        where TState : IBufferProvider<T>
    {
        internal readonly StatelessList<TState, T> _list;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatelessReadOnlyList([NotNull] StatelessList<TState, T> list)
        {
            _list = list;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _list.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StatelessReadOnlyList<TState, T>(StatelessList<TState, T> list)
            => new(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator FasterReadOnlyListRef<T>(StatelessReadOnlyList<TState, T> list)
            => new(list._list._buffer, list._list._count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterListEnumerator<T> GetEnumerator()
            => _list.GetEnumerator();

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _list.ElementAt(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
            => _list.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
            => _list.AsReadOnlySpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory()
            => _list.AsMemory();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> AsReadOnlyMemory()
            => _list.AsReadOnlyMemory();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
            => _list.CopyTo(array, arrayIndex);
    }
}