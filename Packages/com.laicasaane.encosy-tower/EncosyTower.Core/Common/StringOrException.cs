using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Common
{
    public readonly struct StringOrException : IEquatable<StringOrException>, IUnion, ISpanFormattable, IHasValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringOrException(string str)
        {
            Value = str;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringOrException(Exception exception)
        {
            Value = exception;
        }

        public bool HasValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value is not null;
        }

        public object Value { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringOrException(string str)
            => new(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringOrException(Exception exception)
            => new(exception);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(StringOrException other)
            => Value switch {
                string str => Equals(str, other.Value),
                Exception ex => Equals(ex, other.Value),
                _ => false,
            };

        public override bool Equals(object obj)
            => obj switch {
                StringOrException other => Equals(other),
                string str => Equals(str, Value),
                Exception ex => Equals(ex, Value),
                _ => false,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Value?.GetHashCode() ?? 0;

        public override string ToString()
            => Value switch {
                string str => str,
                Exception ex => ex.ToString(),
                _ => string.Empty,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider)
            => ToString();

        public bool TryGetValue(out string str)
        {
            if (Value is string s)
            {
                str = s;
                return true;
            }

            str = default;
            return false;
        }

        public bool TryGetValue(out Exception exception)
        {
            if (Value is Exception ex)
            {
                exception = ex;
                return true;
            }

            exception = default;
            return false;
        }

        public bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        )
        {
            ReadOnlySpan<char> source = Value switch {
                string str => str.AsSpan(),
                Exception ex => ex.ToString().AsSpan(),
                _ => ReadOnlySpan<char>.Empty,
            };

            if (destination.Length >= source.Length)
            {
                source.CopyTo(destination);
                charsWritten = source.Length;
                return true;
            }

            charsWritten = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Equals(string a, object b)
            => b is string str && string.Equals(a, str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Equals(Exception a, object b)
            => b is Exception ex && Equals(a, ex);
    }
}
