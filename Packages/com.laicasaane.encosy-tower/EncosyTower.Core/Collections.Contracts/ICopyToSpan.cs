using System;

namespace EncosyTower.Collections
{
    /// <example>
    /// <code>
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public void CopyTo(Span&lt;T&gt; destination)
    ///     => CopyTo(0, destination);
    ///
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public void CopyTo(Span&lt;T&gt; destination, int length)
    ///     => CopyTo(0, destination, length);
    ///
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public void CopyTo(int sourceStartIndex, Span&lt;T&gt; destination)
    ///     => CopyTo(sourceStartIndex, destination, destination.Length);
    ///
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public void CopyTo(int sourceStartIndex, Span&lt;T&gt; destination, int length)
    ///     => AsReadOnlySpan().Slice(sourceStartIndex, length).CopyTo(destination[..length]);
    /// </code>
    /// </example>
    public interface ICopyToSpan<T>
    {
        void CopyTo(Span<T> destination);

        void CopyTo(Span<T> destination, int length);

        void CopyTo(int sourceStartIndex, Span<T> destination);

        void CopyTo(int sourceStartIndex, Span<T> destination, int length);
    }

    /// <example>
    /// <code>
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public bool TryCopyTo(Span&lt;T&gt; destination)
    ///     => TryCopyTo(0, destination);
    ///
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public bool TryCopyTo(Span&lt;T&gt; destination, int length)
    ///     => TryCopyTo(0, destination, length);
    ///
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public bool TryCopyTo(int sourceStartIndex, Span&lt;T&gt; destination)
    ///     => TryCopyTo(sourceStartIndex, destination, destination.Length);
    ///
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public bool TryCopyTo(int sourceStartIndex, Span&lt;T&gt; destination, int length)
    ///     => AsReadOnlySpan().Slice(sourceStartIndex, length).TryCopyTo(destination[..length]);
    /// </code>
    /// </example>
    public interface ITryCopyToSpan<T>
    {
        bool TryCopyTo(Span<T> destination);

        bool TryCopyTo(Span<T> destination, int length);

        bool TryCopyTo(int sourceStartIndex, Span<T> destination);

        bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length);
    }
}
