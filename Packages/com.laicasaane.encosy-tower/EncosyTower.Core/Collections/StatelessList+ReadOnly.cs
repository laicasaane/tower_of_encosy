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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.Collections
{
    partial struct StatelessList<TState, T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly()
            => new(this);

        public readonly struct ReadOnly : IReadOnlyList<T>, IAsReadOnlySpan<T>
            , ICopyToSpan<T>, ITryCopyToSpan<T>
        {
            internal readonly StatelessList<TState, T> _list;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly([NotNull] StatelessList<TState, T> list)
            {
                _list = list;
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _list.IsCreated;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _list.Count;
            }
            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _list.Capacity;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(StatelessList<TState, T> list)
                => new(list);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BufferProviderEnumerator<T> GetEnumerator()
                => _list.GetEnumerator();

            public T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _list[index];
            }

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
            public void CopyTo(T[] destination, int destinationIndex)
                => _list.CopyTo(destination, destinationIndex);

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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
                => GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }
    }
}
