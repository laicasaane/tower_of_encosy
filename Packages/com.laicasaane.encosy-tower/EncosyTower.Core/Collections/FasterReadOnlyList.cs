// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/Arrays/FasterReadOnlyList.cs

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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.Collections
{
    public readonly struct FasterReadOnlyList<T> : IAsSpan<T>, IAsReadOnlySpan<T>
    {
        internal readonly FasterList<T> _list;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterReadOnlyList([NotNull] FasterList<T> list)
        {
            _list = list;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _list != null;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _list._count;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _list.Capacity;
        }

        public bool IsReadOnly => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator FasterReadOnlyList<T>(FasterList<T> list)
            => new(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterListEnumerator<T> GetEnumerator()
            => _list.GetEnumerator();

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _list[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
            => _list.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
            => _list.AsReadOnlySpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T item)
            => _list.Contains(in item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
            => _list.Contains(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, Span<T> array)
            => _list.CopyTo(index, array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, Span<T> array, int length)
            => _list.CopyTo(index, array, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, T[] array, int arrayIndex, int length)
            => _list.CopyTo(index, array, arrayIndex, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> array)
            => _list.CopyTo(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array)
            => _list.CopyTo(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
            => _list.CopyTo(array, arrayIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists([NotNull] Predicate<T> match)
            => _list.Exists(match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists([NotNull] PredicateIn<T> match)
            => _list.Exists(match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> Find([NotNull] Predicate<T> match)
            => _list.Find(match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> Find([NotNull] PredicateIn<T> match)
            => _list.Find(match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterList<T> FindAll([NotNull] Predicate<T> match)
            => _list.FindAll(match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterList<T> FindAll([NotNull] PredicateIn<T> match)
            => _list.FindAll(match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FindAll([NotNull] Predicate<T> match, [NotNull] FasterList<T> result)
            => _list.FindAll(match, result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FindAll([NotNull] PredicateIn<T> match, [NotNull] FasterList<T> result)
            => _list.FindAll(match, result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FindAll([NotNull] Predicate<T> match, [NotNull] ICollection<T> result)
            => _list.FindAll(match, result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FindAll([NotNull] PredicateIn<T> match, [NotNull] ICollection<T> result)
            => _list.FindAll(match, result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindIndex(int startIndex, int count, [NotNull] Predicate<T> match)
            => _list.FindIndex(startIndex, count, match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindIndex(int startIndex, int count, [NotNull] PredicateIn<T> match)
            => _list.FindIndex(startIndex, count, match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindIndex(int startIndex, Predicate<T> match)
            => _list.FindIndex(startIndex, match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindIndex(int startIndex, PredicateIn<T> match)
            => _list.FindIndex(startIndex, match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindIndex([NotNull] Predicate<T> match)
            => _list.FindIndex(match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindIndex([NotNull] PredicateIn<T> match)
            => _list.FindIndex(match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindLastIndex(int startIndex, int count, [NotNull] Predicate<T> match)
            => _list.FindLastIndex(startIndex, count, match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindLastIndex(int startIndex, int count, [NotNull] PredicateIn<T> match)
            => _list.FindLastIndex(startIndex, count, match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindLastIndex(int startIndex, [NotNull] Predicate<T> match)
            => _list.FindLastIndex(startIndex, match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindLastIndex(int startIndex, [NotNull] PredicateIn<T> match)
            => _list.FindLastIndex(startIndex, match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindLastIndex([NotNull] Predicate<T> match)
            => _list.FindLastIndex(match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindLastIndex([NotNull] PredicateIn<T> match)
            => _list.FindLastIndex(match);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach([NotNull] Action<T> action)
            => _list.ForEach(action);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach([NotNull] ActionIn<T> action)
            => _list.ForEach(action);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach([NotNull] ActionRef<T> action)
            => _list.ForEach(action);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item)
            => _list.IndexOf(in item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index)
            => _list.IndexOf(in item, index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index, int count)
            => _list.IndexOf(in item, index, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item)
            => _list.IndexOf(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item, int index)
            => _list.IndexOf(item, index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item, int index, int count)
            => _list.IndexOf(item, index, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
            => _list.ToArray();
    }
}
