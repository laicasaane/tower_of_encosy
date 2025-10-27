using System;

namespace EncosyTower.Collections
{
    /// <example>
    /// <code>
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public void CopyFrom(ReadOnlySpan&lt;T&gt; source)
    ///     => CopyFrom(0, source);
    ///
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public void CopyFrom(ReadOnlySpan&lt;T&gt; source, int length)
    ///     => CopyFrom(0, source, length);
    ///
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public void CopyFrom(int destinationStartIndex, ReadOnlySpan&lt;T&gt; source)
    ///     => CopyFrom(destinationStartIndex, source, source.Length);
    ///
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public void CopyFrom(int destinationStartIndex, ReadOnlySpan&lt;T&gt; source, int length)
    ///     => source[..length].CopyTo(AsSpan().Slice(destinationStartIndex, length));
    /// </code>
    /// </example>
    public interface ICopyFromSpan<T>
    {
        void CopyFrom(ReadOnlySpan<T> source);

        void CopyFrom(ReadOnlySpan<T> source, int length);

        void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source);

        void CopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length);
    }

    /// <example>
    /// <code>
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public bool TryCopyFrom(ReadOnlySpan&lt;T&gt; source)
    ///     => TryCopyFrom(0, source);
    ///
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public bool TryCopyFrom(ReadOnlySpan&lt;T&gt; source, int length)
    ///     => TryCopyFrom(0, source, length);
    ///
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan&lt;T&gt; source)
    ///     => TryCopyFrom(destinationStartIndex, source, source.Length);
    ///
    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// public bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan&lt;T&gt; source, int length)
    ///     => source[..length].TryCopyTo(AsSpan().Slice(destinationStartIndex, length));
    /// </code>
    /// </example>
    public interface ITryCopyFromSpan<T>
    {
        bool TryCopyFrom(ReadOnlySpan<T> source);

        bool TryCopyFrom(ReadOnlySpan<T> source, int length);

        bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source);

        bool TryCopyFrom(int destinationStartIndex, ReadOnlySpan<T> source, int length);
    }
}
