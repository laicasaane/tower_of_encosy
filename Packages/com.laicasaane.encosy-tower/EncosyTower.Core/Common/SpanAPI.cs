using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Common
{
#if NET9_0_OR_GREATER
    using API = System.MemoryExtensions;
#else
    using API = System.Buffers.SpanSplitExtensions;
#endif

    /// <summary>
    /// Provides extended functionality for spans.
    /// </summary>
    public static class SpanAPI
    {
        /// <summary>
        /// Splits the source span by the specified separator.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="source">The source span to split.</param>
        /// <param name="separator">The separator element.</param>
        /// <returns>An enumerator over the split ranges.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static API.SpanSplitEnumerator<T> Split<T>(this ReadOnlySpan<T> source, T separator)
            where T : IEquatable<T>
        {
            return API.Split(source, separator);
        }

        /// <summary>
        /// Splits the source span by the specified separator sequence.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="source">The source span to split.</param>
        /// <param name="separator">The separator sequence.</param>
        /// <returns>An enumerator over the split ranges.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static API.SpanSplitEnumerator<T> Split<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> separator)
            where T : IEquatable<T>
        {
            return API.Split(source, separator);
        }

        /// <summary>
        /// Splits the source span by any of the specified separator elements.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="source">The source span to split.</param>
        /// <param name="separators">The separator elements.</param>
        /// <returns>An enumerator over the split ranges.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static API.SpanSplitEnumerator<T> SplitAny<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> separators)
            where T : IEquatable<T>
        {
            return API.SplitAny(source, separators);
        }

        /// <summary>
        /// Splits the source span by the specified separator, limiting the number of splits.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="source">The source span to split.</param>
        /// <param name="separator">The separator element.</param>
        /// <param name="count">The maximum number of split results to return.</param>
        /// <returns>A counted enumerator over the split ranges.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CountedSplitEnumerator<T> Split<T>(this ReadOnlySpan<T> source, T separator, int count)
            where T : IEquatable<T>
        {
            return new(source, Split(source, separator), count);
        }

        /// <summary>
        /// Splits the source span by the specified separator sequence, limiting the number of splits.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="source">The source span to split.</param>
        /// <param name="separator">The separator sequence.</param>
        /// <param name="count">The maximum number of split results to return.</param>
        /// <returns>A counted enumerator over the split ranges.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CountedSplitEnumerator<T> Split<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> separator, int count)
            where T : IEquatable<T>
        {
            return new(source, Split(source, separator), count);
        }

        /// <summary>
        /// Splits the source span by any of the specified separator elements, limiting the number of splits.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="source">The source span to split.</param>
        /// <param name="separators">The separator elements.</param>
        /// <param name="count">The maximum number of split results to return.</param>
        /// <returns>A counted enumerator over the split ranges.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CountedSplitEnumerator<T> SplitAny<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> separators, int count)
            where T : IEquatable<T>
        {
            return new(source, SplitAny(source, separators), count);
        }

        /// <summary>
        /// A ref struct enumerator that wraps a span split enumerator and limits the number of returned splits.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        public ref struct CountedSplitEnumerator<T> where T : IEquatable<T>
        {
            private readonly ReadOnlySpan<T> _span;
            private API.SpanSplitEnumerator<T> _enumerator;
            private readonly int _count;
            private int _counter;

            /// <summary>
            /// Initializes a new instance of the <see cref="CountedSplitEnumerator{T}"/> struct.
            /// </summary>
            /// <param name="span">The original span being split.</param>
            /// <param name="enumerator">The underlying span split enumerator.</param>
            /// <param name="count">The maximum number of splits to enumerate.</param>
            internal CountedSplitEnumerator(ReadOnlySpan<T> span, API.SpanSplitEnumerator<T> enumerator, int count)
            {
                _span = span;
                _enumerator = enumerator;
                _count = count;
                _counter = -1;
            }

            /// <summary>
            /// Gets the current range in the split enumeration.
            /// For the last split, the range extends to the end of the span.
            /// </summary>
            public readonly Range Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var current = _enumerator.Current;
                    var lastCounter = _count - 1;
                    return _counter < lastCounter
                        ? current
                        : new(current.Start, _span.Length);
                }
            }

            /// <summary>
            /// Returns this enumerator instance.
            /// </summary>
            /// <returns>This enumerator.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly CountedSplitEnumerator<T> GetEnumerator()
            {
                return this;
            }

            /// <summary>
            /// Moves to the next split range.
            /// </summary>
            /// <returns>True if there is a next split within the count limit; otherwise, false.</returns>
            public bool MoveNext()
            {
                var moved = _enumerator.MoveNext();
                var counter = _counter += moved ? 1 : 0;
                return moved && ((uint)counter < (uint)_count);
            }

            /// <summary>
            /// Resets the enumerator to its initial state.
            /// </summary>
            public void Reset()
            {
                _counter = 1;
            }
        }
    }
}
