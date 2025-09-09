#if !NET9_0_OR_GREATER

using System.Runtime.CompilerServices;

namespace System.Buffers
{
    public static partial class SpanSplitExtensions
    {
        public static SpanSplitEnumerator<T> Split<T>(this ReadOnlySpan<T> source, T separator)
            where T : IEquatable<T>
        {
            return new SpanSplitEnumerator<T>(source, separator);
        }

        public static SpanSplitEnumerator<T> Split<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> separator)
            where T : IEquatable<T>
        {
            return new SpanSplitEnumerator<T>(source, separator, SpanSplitEnumeratorMode.Sequence);
        }

        public static SpanSplitEnumerator<T> SplitAny<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> separators)
            where T : IEquatable<T>
        {
            return new SpanSplitEnumerator<T>(source, separators, SpanSplitEnumeratorMode.Any);
        }

        public static SpanSplitEnumerator<T> Split<T>(this Span<T> source, T separator)
            where T : IEquatable<T>
        {
            return new SpanSplitEnumerator<T>(source, separator);
        }

        public static SpanSplitEnumerator<T> Split<T>(this Span<T> source, ReadOnlySpan<T> separator)
            where T : IEquatable<T>
        {
            return new SpanSplitEnumerator<T>(source, separator, SpanSplitEnumeratorMode.Sequence);
        }

        public static SpanSplitEnumerator<T> SplitAny<T>(this Span<T> source, ReadOnlySpan<T> separators)
            where T : IEquatable<T>
        {
            return new SpanSplitEnumerator<T>(source, separators, SpanSplitEnumeratorMode.Any);
        }

        /// <summary>
        /// Enables enumerating each split within a <see cref="ReadOnlySpan{T}"/>
        /// that has been divided using one or more separators.
        /// </summary>
        public ref struct SpanSplitEnumerator<T> where T : IEquatable<T>
        {
            private readonly ReadOnlySpan<T> _span;
            private readonly T _delimiter;
            private readonly ReadOnlySpan<T> _delimiterSpan;
            private SpanSplitEnumeratorMode _mode;

#if NET8_0
            private readonly SearchValues<T> _searchValues = null!;
#endif

            private int _currentStartIndex;
            private int _currentEndIndex;
            private int _nextStartIndex;

            internal SpanSplitEnumerator(ReadOnlySpan<T> source, T delimiter)
            {
                _span = source;
                _delimiter = delimiter;
                _delimiterSpan = default;
                _mode = SpanSplitEnumeratorMode.Delimiter;
                _currentStartIndex = 0;
                _currentEndIndex = 0;
                _nextStartIndex = 0;
            }

            internal SpanSplitEnumerator(
                  ReadOnlySpan<T> source
                , ReadOnlySpan<T> delimiter
                , SpanSplitEnumeratorMode mode
            )
            {
                _span = source;
                _delimiterSpan = delimiter;
                _delimiter = default!;
                _mode = mode;
                _currentStartIndex = 0;
                _currentEndIndex = 0;
                _nextStartIndex = 0;
            }

#if NET8_0
            internal SpanSplitEnumerator(ReadOnlySpan<T> source, SearchValues<T> searchValues)
            {
                _span = source;
                _delimiter = default!;
                _searchValues = searchValues;
                _delimiterSpan = default;
                _mode = SpanSplitEnumeratorMode.Delimiter;
                _currentStartIndex = 0;
                _currentEndIndex = 0;
                _nextStartIndex = 0;
            }
#endif

            /// <summary>
            /// Gets the current element of the enumeration.
            /// </summary>
            /// <returns>
            /// Returns a <see cref="Range"/> instance that indicates the bounds
            /// of the current element within the source span.
            /// </returns>
            public readonly Range Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new(_currentStartIndex, _currentEndIndex);
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly SpanSplitEnumerator<T> GetEnumerator()
            {
                return this;
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if the enumerator was successfully advanced to the next element;
            /// <br/>
            /// <see langword="false"/> if the enumerator has passed the end of the collection.
            /// </returns>
            public bool MoveNext()
            {
                int index;
                int length;

                switch (_mode)
                {
                    case SpanSplitEnumeratorMode.Delimiter:
                        index = _span[_nextStartIndex..].IndexOf(_delimiter);
                        length = 1;
                        break;

                    case SpanSplitEnumeratorMode.Any:
                        index = _span[_nextStartIndex..].IndexOfAny(_delimiterSpan);
                        length = 1;
                        break;

                    case SpanSplitEnumeratorMode.Sequence:
                        index = _span[_nextStartIndex..].IndexOf(_delimiterSpan);
                        length = _delimiterSpan.Length;
                        break;

                    case SpanSplitEnumeratorMode.EmptySequence:
                        index = -1;
                        length = 1;
                        break;

#if NET8_0
                    case SpanSplitEnumeratorMode.SearchValues:
                        index = _span[_nextStartIndex..].IndexOfAny(_searchValues);
                        length = 1;
                        break;
#endif
                    default:
                        return false;
                }

                _currentStartIndex = _nextStartIndex;

                if (index < 0)
                {
                    _currentEndIndex = _span.Length;
                    _nextStartIndex = _span.Length;

                    _mode = (SpanSplitEnumeratorMode)(-1);
                    return true;
                }

                _currentEndIndex = _currentStartIndex + index;
                _nextStartIndex = _currentEndIndex + length;

                return true;
            }
        }

        internal enum SpanSplitEnumeratorMode
        {
            Delimiter,
            Any,
            Sequence,
            EmptySequence,
            SearchValues
        }
    }
}

#endif
