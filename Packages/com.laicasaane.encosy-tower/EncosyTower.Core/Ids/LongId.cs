
namespace EncosyTower.Ids
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using EncosyTower.Conversion;
    using EncosyTower.Serialization;
    using UnityEngine;

    [TypeConverter(typeof(TypeConverter))]
    public readonly partial struct LongId
        : IEquatable<LongId>
        , IComparable<LongId>
        , ITryParse<LongId>
        , ITryParseSpan<LongId>
        , ISpanFormattable
    {
        private readonly ulong _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LongId(ulong value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(LongId other)
            => _value.CompareTo(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(LongId other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is LongId other && _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider)
            => _value.ToString(format, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        )
        {
            return _value.TryFormat(destination, out charsWritten, format, provider);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryParse(
              string str
            , out LongId result
            , bool ignoreCase
            , bool allowMatchingMetadataAttribute
        )
        {
            return TryParse(str.AsSpan(), out result, ignoreCase, allowMatchingMetadataAttribute);
        }

        public bool TryParse(
              ReadOnlySpan<char> str
            , out LongId result
            , bool ignoreCase
            , bool allowMatchingMetadataAttribute
        )
        {
            if (str.IsEmpty)
            {
                goto FAILED;
            }

            if (ulong.TryParse(str, out var value))
            {
                result = new(value);
                return true;
            }

        FAILED:
            result = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator LongId(ulong value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ulong(in LongId value)
            => value._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in LongId left, in LongId right)
            => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in LongId left, in LongId right)
            => left._value != right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(in LongId left, in LongId right)
            => left._value < right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(in LongId left, in LongId right)
            => left._value > right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(in LongId left, in LongId right)
            => left._value <= right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(in LongId left, in LongId right)
            => left._value >= right._value;

        public sealed class TypeConverter : ParsableStructConverter<LongId>
        {
            public override bool IgnoreCase => false;

            public override bool AllowMatchingMetadataAttribute => false;
        }

        [Serializable]
        public partial struct Serializable : ITryConvert<LongId>
            , IEquatable<Serializable>
            , IComparable<Serializable>
            , ISpanFormattable
        {
            [SerializeField]
            private ulong _value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Serializable(ulong value)
            {
                _value = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryConvert(out LongId result)
            {
                result = new(_value);
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Equals(Serializable other)
                => _value == other._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override bool Equals(object obj)
                => obj is Serializable other && _value == other._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override int GetHashCode()
                => _value.GetHashCode();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override string ToString()
                => _value.ToString();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public string ToString(string format, IFormatProvider formatProvider)
                => _value.ToString(format, formatProvider);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly int CompareTo(Serializable other)
                => _value.CompareTo(other._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryFormat(
                  Span<char> destination
                , out int charsWritten
                , ReadOnlySpan<char> format
                , IFormatProvider provider
            )
            {
                return _value.TryFormat(destination, out charsWritten, format, provider);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator LongId(Serializable value)
                => new(value._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(LongId value)
                => new(value._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable left, Serializable right)
                => left._value == right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable left, Serializable right)
                => left._value != right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator <(Serializable left, Serializable right)
                => left._value < right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator <=(Serializable left, Serializable right)
                => left._value <= right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator >(Serializable left, Serializable right)
                => left._value > right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator >=(Serializable left, Serializable right)
                => left._value >= right._value;
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Ids
{
    using System.Runtime.CompilerServices;
    using EncosyTower.Conversion;
    using Unity.Collections;

    public readonly partial struct LongId : IToFixedString<FixedString32Bytes>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly FixedString32Bytes ToFixedString()
        {
            var fs = new FixedString32Bytes();
            fs.Append(_value);
            return fs;
        }

        public partial struct Serializable : IToFixedString<FixedString32Bytes>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly FixedString32Bytes ToFixedString()
            {
                var fs = new FixedString32Bytes();
                fs.Append(_value);
                return fs;
            }
        }
    }
}

#endif
