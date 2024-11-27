// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/Arrays/FasterListEnumerator.cs

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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;

namespace EncosyTower.Modules.Collections
{
    public ref struct FasterListEnumerator<T>
    {
        private readonly T[] _buffer;
        private int _counter;
        private readonly int _size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterListEnumerator([NotNull] T[] buffer, int size)
        {
            _size = size;
            _counter = 0;
            _buffer = buffer;
        }

        public ref T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Assert.IsTrue(_counter <= _size);
                return ref _buffer[_counter - 1];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
            => _counter++ < _size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
            => _counter = 0;
    }
}

namespace EncosyTower.Modules.Collections.Internals
{
    internal struct FasterListEnumerator<T> : IEnumerator<T>
    {
        private readonly T[] _buffer;
        private int _counter;
        private readonly int _size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterListEnumerator(in T[] buffer, int size)
        {
            _size = size;
            _counter = 0;
            _buffer = buffer;
        }

        public T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Assert.IsTrue(_counter <= _size);
                return _buffer[_counter - 1];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
            => _counter++ < _size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
            => _counter = 0;

        object IEnumerator.Current
            => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { }
    }
}