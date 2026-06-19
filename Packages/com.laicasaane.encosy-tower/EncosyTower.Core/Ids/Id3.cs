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
    public partial struct Id3
        : IEquatable<Id3>
        , ITryParse<Id3>
        , ITryParseSpan<Id3>
        , ISpanFormattable
    {
        [SerializeField, HideInInspector]
        private Id _x;

        [SerializeField, HideInInspector]
        private Id _y;

        [SerializeField, HideInInspector]
        private Id _z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Id3(Id x, Id y, Id z) : this()
        {
            _x = x;
            _y = y;
            _z = z;
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

        public readonly Id Z
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Deconstruct(out Id x, out Id y, out Id z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Id3 other)
            => _x == other._x && _y == other._y && _z == other._z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is Id3 other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode()
            => HashValue.Combine(_x, _y, _z);

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

            destination = destination[yChars..];

            if (destination.Length < 2)
            {
                return False(out charsWritten);
            }

            destination[delimiterChars++] = '-';
            destination = destination[delimiterChars..];

            if (_z.TryFormat(destination, out var zChars, format, provider) == false)
            {
                return False(out charsWritten);
            }

            charsWritten = xChars + delimiterChars + yChars + zChars;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string ToString(string format, IFormatProvider formatProvider = null)
            => $"{_x.ToString(format, formatProvider)}-{_y.ToString(format, formatProvider)}-{_z.ToString(format, formatProvider)}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryParse(
              string str
            , out Id3 result
            , bool ignoreCase = true
            , bool allowMatchingMetadataAttribute = false
        )
        {
            return TryParse(str.AsSpan(), out result, ignoreCase, allowMatchingMetadataAttribute);
        }

        public readonly bool TryParse(
              ReadOnlySpan<char> str
            , out Id3 result
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
            Range? zRange = default;

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

                if (zRange.HasValue == false)
                {
                    zRange = range;
                    continue;
                }

                goto FAILED;
            }

            if (xRange.HasValue == false || yRange.HasValue == false || zRange.HasValue == false)
            {
                goto FAILED;
            }

            var xSpan = str[xRange.Value];
            var ySpan = str[yRange.Value];
            var zSpan = str[zRange.Value];

            if (uint.TryParse(xSpan, out var x) && uint.TryParse(ySpan, out var y) && uint.TryParse(zSpan, out var z))
            {
                result = new(x, y, z);
                return true;
            }

        FAILED:
            result = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Id3(in (Id x, Id y, Id z) tuple)
            => new(tuple.x, tuple.y, tuple.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Id3 left, in Id3 right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Id3 left, in Id3 right)
            => !left.Equals(right);

        public sealed class TypeConverter : ParsableStructConverter<Id3>
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

    partial struct Id3 : IToFixedString, IToFixedString<FixedString64Bytes>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => ToFixedString().ToString();

        public readonly FixedString64Bytes ToFixedString()
        {
            var fs = new FixedString64Bytes();
            fs.Append(_x);
            fs.Append('-');
            fs.Append(_y);
            fs.Append('-');
            fs.Append(_z);
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

    partial struct Id3
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => $"{_x}-{_y}-{_z}";
    }
}

#endif
