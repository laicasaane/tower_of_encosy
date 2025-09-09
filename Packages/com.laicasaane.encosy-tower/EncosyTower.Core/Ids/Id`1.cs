namespace EncosyTower.Ids
{
    using System;
    using System.Runtime.CompilerServices;
    using EncosyTower.Conversion;
    using EncosyTower.Serialization;
    using UnityEngine;

    public readonly partial struct Id<T>
        : IEquatable<Id<T>>
        , IComparable<Id<T>>
        , ITryParse<Id<T>>
        , ITryParseSpan<Id<T>>
    {
        private readonly uint _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Id(int value)
        {
            _value = new Id.Union(value).uintValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Id(uint value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Id<T> other)
            => _value.CompareTo(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Id<T> other)
            => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is Id<T> other && _value == other._value;

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
            , out Id<T> result
            , bool ignoreCase
            , bool allowMatchingMetadataAttribute
        )
        {
            return TryParse(str.AsSpan(), out result, ignoreCase, allowMatchingMetadataAttribute);
        }

        public bool TryParse(
              ReadOnlySpan<char> str
            , out Id<T> result
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
        public static implicit operator Id<T>(int value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Id<T>(uint value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Id<T>(Id value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Id(Id<T> value)
            => value._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint(Id<T> value)
            => value._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Id<T> left, Id<T> right)
            => left._value == right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Id<T> left, Id<T> right)
            => left._value != right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Id<T> left, Id<T> right)
            => left._value < right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Id<T> left, Id<T> right)
            => left._value > right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Id<T> left, Id<T> right)
            => left._value <= right._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Id<T> left, Id<T> right)
            => left._value >= right._value;

        public sealed class TypeConverter : ParsableStructConverter<Id>
        {
            public override bool IgnoreCase => false;

            public override bool AllowMatchingMetadataAttribute => false;
        }
    }

    partial struct Id
    {
        [Serializable]
        public partial struct Serializable<T> : ITryConvert<Id<T>>
            , IEquatable<Serializable<T>>
            , IComparable<Serializable<T>>
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
            public readonly bool TryConvert(out Id<T> result)
            {
                result = new(_value);
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Equals(Serializable<T> other)
                => _value == other._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override bool Equals(object obj)
                => obj is Serializable<T> other && _value == other._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override int GetHashCode()
                => _value.GetHashCode();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override string ToString()
                => _value.ToString();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly int CompareTo(Serializable<T> other)
                => _value.CompareTo(other._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable<T>(int value)
                => new(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable<T>(uint value)
                => new(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Id<T>(Serializable<T> value)
                => new(value._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Id(Serializable<T> value)
                => new(value._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable<T>(Serializable value)
                => new(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(Serializable<T> value)
                => new(value._value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable<T>(Id<T> value)
                => new(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator uint(Serializable<T> value)
                => value._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable<T> left, Serializable<T> right)
                => left._value == right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable<T> left, Serializable<T> right)
                => left._value != right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator <(Serializable<T> left, Serializable<T> right)
                => left._value < right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator <=(Serializable<T> left, Serializable<T> right)
                => left._value <= right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator >(Serializable<T> left, Serializable<T> right)
                => left._value > right._value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator >=(Serializable<T> left, Serializable<T> right)
                => left._value >= right._value;
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Ids
{
    using System.Runtime.CompilerServices;
    using Unity.Collections;

    public readonly partial struct Id<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly FixedString32Bytes ToFixedString()
        {
            var fs = new FixedString32Bytes();
            fs.Append(_value);
            return fs;
        }
    }

    partial struct Id
    {
        public partial struct Serializable<T>
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
