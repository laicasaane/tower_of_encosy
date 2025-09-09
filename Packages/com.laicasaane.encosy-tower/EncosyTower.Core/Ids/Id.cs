namespace EncosyTower.Ids
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using EncosyTower.Conversion;
    using EncosyTower.Serialization;
    using UnityEngine;

    [TypeConverter(typeof(TypeConverter))]
    public readonly partial struct Id
        : IEquatable<Id>
        , IComparable<Id>
        , ITryParse<Id>
        , ITryParseSpan<Id>
    {
        private readonly uint _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Id(int value)
        {
            _value = new Union(value).uintValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Id(uint value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Id other)
            => _value.CompareTo(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Id other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is Id other && _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _value.ToString();

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
            , out Id result
            , bool ignoreCase
            , bool allowMatchingMetadataAttribute
        )
        {
            return TryParse(str.AsSpan(), out result, ignoreCase, allowMatchingMetadataAttribute);
        }

        public bool TryParse(
              ReadOnlySpan<char> str
            , out Id result
            , bool ignoreCase
            , bool allowMatchingMetadataAttribute
        )
        {
            if (str.IsEmpty)
            {
                goto FAILED;
            }

            if (uint.TryParse(str, out var value))
            {
                result = new(value);
                return true;
            }

        FAILED:
            result = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Id(int value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Id(uint value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint(Id value)
            => value._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Id left, Id right)
            => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Id left, Id right)
            => left._value != right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Id left, Id right)
            => left._value < right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Id left, Id right)
            => left._value > right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Id left, Id right)
            => left._value <= right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Id left, Id right)
            => left._value >= right._value;

        public sealed class TypeConverter : ParsableStructConverter<Id>
        {
            public override bool IgnoreCase => false;

            public override bool AllowMatchingMetadataAttribute => false;
        }

        [Serializable]
        public partial struct Serializable : ITryConvert<Id>
            , IEquatable<Serializable>
            , IComparable<Serializable>
        {
            [SerializeField]
            private uint _value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Serializable(int value)
            {
                _value = new Union(value).uintValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Serializable(uint value)
            {
                _value = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryConvert(out Id result)
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
            public readonly int CompareTo(Serializable other)
                => _value.CompareTo(other._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(int value)
                => new(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(uint value)
                => new(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Id(Serializable value)
                => new(value._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(Id value)
                => new(value._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator uint(Serializable value)
                => value._value;

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

        [StructLayout(LayoutKind.Explicit)]
        internal struct Union
        {
            [FieldOffset(0)]
            public int intValue;

            [FieldOffset(0)]
            public uint uintValue;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Union(int value) : this()
            {
                intValue = value;
            }
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Ids
{
    using System.Runtime.CompilerServices;
    using Unity.Collections;

    public readonly partial struct Id
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly FixedString32Bytes ToFixedString()
        {
            var fs = new FixedString32Bytes();
            fs.Append(_value);
            return fs;
        }

        public partial struct Serializable
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
