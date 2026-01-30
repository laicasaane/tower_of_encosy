using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.Collections
{
    partial class ArraySet<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly()
        {
            return new(this);
        }

        [DebuggerTypeProxy(typeof(ArraySetDebugProxy<>))]
        public readonly struct ReadOnly : IIsCreated, IReadOnlyCollection<T>, IHasCount
            , ICopyToSpan<T>, ITryCopyToSpan<T>
        {
            private static readonly ReadOnly s_empty = new(new ArraySet<T>());

            internal readonly ArraySet<T> _set;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly([NotNull] ArraySet<T> set)
            {
                _set = set;
            }

            public static ReadOnly Empty
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => s_empty;
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _set != null;
            }

            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _set.Capacity;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _set._freeValueCellIndex;
            }

            public ReadOnlyMemory<T> Items
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _set.Items;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(ArraySet<T> set)
                => set is not null ? set.AsReadOnly() : Empty;

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
                => new CopyToSpan<T>(Items.Span).CopyTo(sourceStartIndex, destination, length);

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
                => new CopyToSpan<T>(Items.Span).TryCopyTo(sourceStartIndex, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ArraySetEnumerator<T> GetEnumerator()
                => _set.GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(T value)
                => _set.Contains(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
                => _set.GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator()
                => _set.GetEnumerator();
        }
    }
}
