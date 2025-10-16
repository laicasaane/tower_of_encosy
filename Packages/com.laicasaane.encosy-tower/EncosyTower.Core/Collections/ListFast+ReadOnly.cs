using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.Collections
{
    partial struct ListFast<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly()
            => new(this);

        public readonly struct ReadOnly : IAsReadOnlySpan<T>, IReadOnlyList<T>
        {
            internal readonly ListFast<T> _list;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly(ListFast<T> list)
            {
                _list = list;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly([NotNull] List<T> list)
            {
                _list = new(list);
            }

            public bool IsValid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _list.IsCreated;
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
            public static implicit operator ReadOnly(ListFast<T> list)
                => new(list);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly([NotNull] List<T> list)
                => new(list);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ListFastEnumerator<T> GetEnumerator()
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
            public ListFast<T> FindAll([NotNull] Predicate<T> match)
                => _list.FindAll(match);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ListFast<T> FindAll([NotNull] PredicateIn<T> match)
                => _list.FindAll(match);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void FindAll([NotNull] Predicate<T> match, ListFast<T> result)
                => _list.FindAll(match, result);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void FindAll([NotNull] PredicateIn<T> match, ListFast<T> result)
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
