namespace EncosyTower.Ids
{
    using System;
    using System.Buffers;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using EncosyTower.Conversion;
    using EncosyTower.Serialization;
    using UnityEngine;

    [StructLayout(LayoutKind.Explicit)]
    [TypeConverter(typeof(TypeConverter))]
    public readonly partial record struct Id2
        : IEquatable<Id2>
        , IComparable<Id2>
        , ITryParse<Id2>
        , ITryParseSpan<Id2>
    {
        [FieldOffset(0)]
        private readonly ulong _value;

        [FieldOffset(0)]
        private readonly Id _y;

        [FieldOffset(4)]
        private readonly Id _x;

        // ReSharper disable once UnusedMember.Local
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Id2(ulong value) : this()
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Id2(Id x, Id y) : this()
        {
            _y = y;
            _x = x;
        }

        public Id X
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _x;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init => _x = value;
        }

        public Id Y
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _y;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init => _y = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out Id x, out Id y)
        {
            x = _x;
            y = _y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Id2 other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Id2 other)
            => _value.CompareTo(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        )
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool False(out int value)
            {
                value = 0;
                return false;
            }

            if (destination.Length < 1)
            {
                return False(out charsWritten);
            }

            var openQuoteChars = 0;
            destination[openQuoteChars++] = '(';
            destination = destination[openQuoteChars..];

            if (_x.TryFormat(destination, out var xChars, format, provider) == false)
            {
                return False(out charsWritten);
            }

            destination = destination[xChars..];

            if (destination.Length < 2)
            {
                return False(out charsWritten);
            }

            var delimiterChars = 0;
            destination[delimiterChars++] = ',';
            destination[delimiterChars++] = ' ';
            destination = destination[delimiterChars..];

            if (_y.TryFormat(destination, out var yChars, format, provider) == false)
            {
                return False(out charsWritten);
            }

            destination = destination[yChars..];

            if (destination.Length < 1)
            {
                return False(out charsWritten);
            }

            var closeQuoteChars = 0;
            destination[closeQuoteChars++] = ')';

            charsWritten = openQuoteChars + xChars + delimiterChars + yChars + closeQuoteChars;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryParse(
              string str
            , out Id2 result
            , bool ignoreCase
            , bool allowMatchingMetadataAttribute
        )
        {
            return TryParse(str.AsSpan(), out result, ignoreCase, allowMatchingMetadataAttribute);
        }

        public bool TryParse(
              ReadOnlySpan<char> str
            , out Id2 result
            , bool ignoreCase
            , bool allowMatchingMetadataAttribute
        )
        {
            if (str.IsEmpty)
            {
                goto FAILED;
            }

            var ranges = str.Split('-');
            Range? xRange = default;
            Range? yRange = default;

            foreach (var range in ranges)
            {
                if (xRange.HasValue == false)
                {
                    xRange = range;
                    continue;
                }

                if (yRange.HasValue == false)
                {
                    yRange = range;
                    continue;
                }

                goto FAILED;
            }

            if (xRange.HasValue == false || yRange.HasValue == false)
            {
                goto FAILED;
            }

            var xSpan = str[xRange.Value];
            var ySpan = str[yRange.Value];

            if (uint.TryParse(xSpan, out var x) && uint.TryParse(ySpan, out var y))
            {
                result = new(x, y);
                return true;
            }

        FAILED:
            result = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ulong(in Id2 id)
            => id._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Id2(in (Id x, Id y) tuple)
            => new(tuple.x, tuple.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Id2 left, in Id2 right)
            => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Id2 left, in Id2 right)
            => left._value != right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(in Id2 left, in Id2 right)
            => left._value > right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(in Id2 left, in Id2 right)
            => left._value >= right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(in Id2 left, in Id2 right)
            => left._value < right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(in Id2 left, in Id2 right)
            => left._value <= right._value;

        public sealed class TypeConverter : ParsableStructConverter<Id2>
        {
            public override bool IgnoreCase => false;

            public override bool AllowMatchingMetadataAttribute => false;
        }

        [Serializable]
        public partial struct Serializable : ITryConvert<Id2>
            , IEquatable<Serializable>
            , IComparable<Serializable>
        {
            [SerializeField]
            private Id.Serializable _x;

            [SerializeField]
            private Id.Serializable _y;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Serializable(Id.Serializable x, Id.Serializable y)
            {
                _x = x;
                _y = y;
            }

            public readonly bool TryConvert(out Id2 result)
            {
                if (_x.TryConvert(out var x) && _y.TryConvert(out var y))
                {
                    result = new(x, y);
                    return true;
                }

                result = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Equals(Serializable other)
                => _x == other._x && _y == other._y;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override bool Equals(object obj)
                => obj is Serializable other && _x == other._x && _y == other._y;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override int GetHashCode()
                => ((Id2)this).GetHashCode();

#if !UNITY_COLLECTIONS
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override string ToString()
                => $"({_x}, {_y})";
#endif

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly int CompareTo(Serializable other)
                => ((Id2)this).CompareTo(other);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Id2(Serializable value)
                => new(value._x, value._y);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(Id2 value)
                => new(value._x, value._y);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable left, Serializable right)
                => left._x == right._x && left._y == right._y;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable left, Serializable right)
                => left._x != right._x || left._y != right._y;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator >(in Serializable left, in Serializable right)
                => (Id2)left > (Id2)right;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator >=(in Serializable left, in Serializable right)
                => (Id2)left >= (Id2)right;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator <(in Serializable left, in Serializable right)
                => (Id2)left < (Id2)right;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator <=(in Serializable left, in Serializable right)
                => (Id2)left <= (Id2)right;
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Ids
{
    using System.Runtime.CompilerServices;
    using Unity.Collections;

    partial record struct Id2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => ToFixedString().ToString();

        public readonly FixedString32Bytes ToFixedString()
        {
            var fs = new FixedString32Bytes();
            fs.Append(_x);
            fs.Append('-');
            fs.Append(_y);
            return fs;
        }

        public partial struct Serializable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override string ToString()
                => ToFixedString().ToString();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly FixedString32Bytes ToFixedString()
            {
                var fs = new FixedString32Bytes();
                fs.Append('(');
                fs.Append(_x);
                fs.Append(',');
                fs.Append(' ');
                fs.Append(_y);
                fs.Append(')');
                return fs;
            }
        }
    }
}

#else

namespace EncosyTower.Ids
{
    using System.Runtime.CompilerServices;

    partial record struct Id2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => $"{X}-{Y}";
    }
}

#endif
