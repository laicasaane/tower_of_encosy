using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic.Exposed;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;

namespace EncosyTower.Collections
{
    public readonly struct HashSetReadOnly<T> : IReadOnlyCollection<T>
    {
        private static readonly HashSetReadOnly<T> s_empty = new(new());

        internal readonly HashSetExposed<T> _set;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashSetReadOnly()
        {
            _set = s_empty._set;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashSetReadOnly([NotNull] HashSet<T> set)
        {
            _set = new(set);
        }

        public static HashSetReadOnly<T> Empty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_empty;
        }

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _set.Set != null;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _set.Count;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _set.Entries.Length;
        }

        public bool IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator HashSetReadOnly<T>(HashSet<T> set)
            => set is not null ? new(set) : Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
            => _set.Set.Contains(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex, int count)
            => _set.Set.CopyTo(array, arrayIndex, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
            => _set.Set.CopyTo(array, arrayIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array)
            => _set.Set.CopyTo(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> array)
            => CopyTo(array, 0, _set.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> array, int arrayIndex)
            => CopyTo(array, arrayIndex, _set.Count);

        public void CopyTo(Span<T> array, int arrayIndex, int count)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_IfNegative(arrayIndex, nameof(arrayIndex));
            ThrowHelper.ThrowArgumentOutOfRangeException_IfNegative(count, nameof(count));

            // Will the array, starting at arrayIndex, be able to hold elements? Note: not
            // checking arrayIndex >= array.Length (consistency with list of allowing
            // count of 0; subsequent check takes care of the rest)
            if (arrayIndex > array.Length || count > array.Length - arrayIndex)
            {
                ThrowHelper.ThrowArgumentException_ArrayPlusOffTooSmall();
            }

            var entries = _set.Entries;
            var setCount = _set.Count;

            for (int i = 0; i < setCount && count != 0; i++)
            {
                ref readonly var entry = ref entries[i];

                if (entry.next >= -1)
                {
                    array[arrayIndex++] = entry.value;
                    count--;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj switch {
                HashSetReadOnly<T> other => ReferenceEquals(_set.Set, other._set.Set),
                HashSet<T> other => ReferenceEquals(_set.Set, other),
                _ => false,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _set.Set.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashSet<T>.Enumerator GetEnumerator()
            => _set.Set.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsProperSubsetOf(HashSetReadOnly<T> other)
            => _set.Set.IsProperSubsetOf(other._set.Set);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsProperSupersetOf(HashSetReadOnly<T> other)
            => _set.Set.IsProperSupersetOf(other._set.Set);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSubsetOf(HashSetReadOnly<T> other)
            => _set.Set.IsSubsetOf(other._set.Set);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSupersetOf(HashSetReadOnly<T> other)
            => _set.Set.IsSupersetOf(other._set.Set);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(HashSetReadOnly<T> other)
            => _set.Set.Overlaps(other._set.Set);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetEquals(HashSetReadOnly<T> other)
            => _set.Set.SetEquals(other._set.Set);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsProperSubsetOf(IEnumerable<T> other)
            => _set.Set.IsProperSubsetOf(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsProperSupersetOf(IEnumerable<T> other)
            => _set.Set.IsProperSupersetOf(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSubsetOf(IEnumerable<T> other)
            => _set.Set.IsSubsetOf(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSupersetOf(IEnumerable<T> other)
            => _set.Set.IsSupersetOf(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(IEnumerable<T> other)
            => _set.Set.Overlaps(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetEquals(IEnumerable<T> other)
            => _set.Set.SetEquals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue)
            => _set.Set.TryGetValue(equalValue, out actualValue);
    }
}
