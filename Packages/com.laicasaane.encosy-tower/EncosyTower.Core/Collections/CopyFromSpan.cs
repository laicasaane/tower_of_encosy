using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections
{
    public readonly ref struct CopyFromSpan<T>
    {
        private readonly Span<T> _span;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CopyFromSpan(Span<T> span)
        {
            _span = span;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _span.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator CopyFromSpan<T>(Span<T> span)
            => new(span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(CopyFromSpan<T> span)
            => span._span;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CopyFromSpan<T> Slice(int start)
            => new(_span[start..]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CopyFromSpan<T> Slice(int start, int length)
            => new(_span.Slice(start, length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySlice(int start, out CopyFromSpan<T> span)
        {
            if ((uint)start > (uint)Length)
            {
                span = this;
                return false;
            }

            span = this[start..];
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySlice(int start, int length, out CopyFromSpan<T> span)
        {
            if ((uint)start > (uint)Length || (uint)length > (uint)(Length - start))
            {
                span = this;
                return false;
            }

            span = Slice(start, length);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ReadOnlySpan<T> source)
            => CopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ReadOnlySpan<T> source, int length)
            => CopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => CopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
            => source[..length].CopyTo(Slice(destinationStartIndex, length)._span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(ReadOnlySpan<T> source)
            => TryCopyFrom(0, source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(ReadOnlySpan<T> source, int length)
            => TryCopyFrom(0, source, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source)
            => TryCopyFrom(destinationStartIndex, source, source.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length)
        {
            if ((uint)destinationStartIndex > (uint)source.Length)
            {
                return false;
            }

            if (TrySlice(destinationStartIndex, length, out var dest) == false)
            {
                return false;
            }

            return source[..length].TryCopyTo(dest);
        }
    }
}
