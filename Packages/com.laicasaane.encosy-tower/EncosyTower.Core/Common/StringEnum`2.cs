using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Common
{
    /// <summary>
    /// Provides facilities to serialize and deserialize enum as string.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to be serialized as string.</typeparam>
    /// <typeparam name="TConverter">The converter to provide the specialized conversion APIs.</typeparam>
    [Serializable]
    public struct StringEnum<TEnum, TConverter> : IEquatable<StringEnum<TEnum, TConverter>>, IEquatable<TEnum>
        where TEnum : unmanaged, Enum
        where TConverter : struct, IStringEnumConverter<TEnum>
    {
        [SerializeField, HideInInspector] internal string _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringEnum(TEnum value)
        {
            _value = new TConverter().Transform(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal StringEnum(string value)
        {
            _value = value;
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value.IsNotEmpty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringEnum<TEnum, TConverter>(StringEnum<TEnum> value)
            => new(value._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringEnum<TEnum>(StringEnum<TEnum, TConverter> value)
            => new(value._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringEnum<TEnum, TConverter>(TEnum value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator TEnum(StringEnum<TEnum, TConverter> value)
            => value.ToEnum();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(StringEnum<TEnum, TConverter> lhs, TEnum rhs)
            => lhs.Equals(rhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(StringEnum<TEnum, TConverter> lhs, TEnum rhs)
            => !lhs.Equals(rhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(TEnum lhs, StringEnum<TEnum, TConverter> rhs)
            => rhs.Equals(lhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(TEnum lhs, StringEnum<TEnum, TConverter> rhs)
            => !rhs.Equals(lhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(StringEnum<TEnum, TConverter> lhs, StringEnum<TEnum, TConverter> rhs)
            => lhs.Equals(rhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(StringEnum<TEnum, TConverter> lhs, StringEnum<TEnum, TConverter> rhs)
            => !lhs.Equals(rhs);

        public readonly override bool Equals(object obj)
            => obj switch {
                StringEnum<TEnum, TConverter> other => Equals(other),
                TEnum other => Equals(other),
                string other => string.Equals(_value, other, StringComparison.Ordinal),
                _ => false,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(StringEnum<TEnum, TConverter> other)
            => string.Equals(_value, other._value, StringComparison.Ordinal);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(TEnum other)
            => Equals((StringEnum<TEnum, TConverter>)other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => HashValue.Combine(_value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly TEnum ToEnum()
            => new TConverter().Transform(_value ?? string.Empty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => _value;
    }
}
