using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections
{
    public readonly ref struct CopyToSpan<T>
    {
        private readonly ReadOnlySpan<T> _span;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CopyToSpan(ReadOnlySpan<T> span)
        {
            _span = span;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _span.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator CopyToSpan<T>(ReadOnlySpan<T> span)
            => new(span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(CopyToSpan<T> span)
            => span._span;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CopyToSpan<T> Slice(int start)
            => new(_span[start..]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CopyToSpan<T> Slice(int start, int length)
            => new(_span.Slice(start, length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySlice(int start, out CopyToSpan<T> span)
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
        public bool TrySlice(int start, int length, out CopyToSpan<T> span)
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
        public void CopyTo(Span<T> destination)
            => CopyTo(destination, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> destination, int length)
            => CopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int sourceStartIndex, Span<T> destination)
            => CopyTo(sourceStartIndex, destination, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int sourceStartIndex, Span<T> destination, int length)
            => Slice(sourceStartIndex, length)._span.CopyTo(destination[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<T> destination)
            => TryCopyTo(destination, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<T> destination, int length)
            => TryCopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(int sourceStartIndex, Span<T> destination)
            => TryCopyTo(sourceStartIndex, destination, Length);

        public bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
        {
            if (TrySlice(sourceStartIndex, length, out var source) == false)
            {
                return false;
            }

            if ((uint)sourceStartIndex > (uint)destination.Length)
            {
                return false;
            }

            return source.TryCopyTo(destination[..length]);
        }
    }
}
