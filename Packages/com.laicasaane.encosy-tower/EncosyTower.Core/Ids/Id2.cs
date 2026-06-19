namespace EncosyTower.Ids
{
    using System;
    using System.Buffers;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using EncosyTower.Common;
    using EncosyTower.Conversion;
    using EncosyTower.Serialization;
    using UnityEngine;

    [Serializable]
    [TypeConverter(typeof(TypeConverter))]
    public partial struct Id2
        : IEquatable<Id2>
        , ITryParse<Id2>
        , ITryParseSpan<Id2>
        , ISpanFormattable
    {
        [SerializeField, HideInInspector]
        private Id _x;

        [SerializeField, HideInInspector]
        private Id _y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Id2(Id x, Id y) : this()
        {
            _x = x;
            _y = y;
        }

        public readonly Id X
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _x;
        }

        public readonly Id Y
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Deconstruct(out Id x, out Id y)
        {
            x = _x;
            y = _y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Id2 other)
            => _x == other._x && _y == other._y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is Id2 other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode()
            => HashValue.Combine(_x, _y);

        public readonly bool TryFormat(
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
            destination[delimiterChars++] = '-';
            destination = destination[delimiterChars..];

            if (_y.TryFormat(destination, out var yChars, format, provider) == false)
            {
                return False(out charsWritten);
            }

            charsWritten = xChars + delimiterChars + yChars;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string ToString(string format, IFormatProvider formatProvider = null)
            => $"{_x.ToString(format, formatProvider)}-{_y.ToString(format, formatProvider)}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryParse(
              string str
            , out Id2 result
            , bool ignoreCase = true
            , bool allowMatchingMetadataAttribute = false
        )
        {
            return TryParse(str.AsSpan(), out result, ignoreCase, allowMatchingMetadataAttribute);
        }

        public readonly bool TryParse(
              ReadOnlySpan<char> str
            , out Id2 result
            , bool ignoreCase = true
            , bool allowMatchingMetadataAttribute = false
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
        public static implicit operator Id2(in (Id x, Id y) tuple)
            => new(tuple.x, tuple.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Id2 left, in Id2 right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Id2 left, in Id2 right)
            => !left.Equals(right);

        public sealed class TypeConverter : ParsableStructConverter<Id2>
        {
            public override bool IgnoreCase => false;

            public override bool AllowMatchingMetadataAttribute => false;
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Ids
{
    using System.Runtime.CompilerServices;
    using EncosyTower.Collections;
    using EncosyTower.Conversion;
    using Unity.Collections;

    partial struct Id2 : IToFixedString, IToFixedString<FixedString32Bytes>
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly TFixedString ToFixedString<TFixedString>()
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
            => ToFixedString().CastTo<TFixedString>();
    }
}

#else

namespace EncosyTower.Ids
{
    using System;
    using System.Runtime.CompilerServices;

    partial struct Id2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => $"{_x}-{_y}";
    }
}

#endif
