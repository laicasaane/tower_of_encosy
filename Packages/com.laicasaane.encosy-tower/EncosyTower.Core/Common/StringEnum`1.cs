using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Common
{
    /// <summary>
    /// Provides facilities to serialize and deserialize enum as string.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to be serialized as string.</typeparam>
    [Serializable]
    public struct StringEnum<TEnum> :IEquatable<StringEnum<TEnum>>, IEquatable<TEnum>
        where TEnum : unmanaged, Enum
    {
        [SerializeField, HideInInspector] internal string _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringEnum(TEnum value)
        {
            _value = value.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringEnum(string value)
        {
            _value = value;
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value.IsNotEmpty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringEnum<TEnum>(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(StringEnum<TEnum> value)
            => value._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringEnum<TEnum>(TEnum value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator TEnum(StringEnum<TEnum> value)
            => value.ToEnum();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(StringEnum<TEnum> lhs, TEnum rhs)
            => lhs.Equals(rhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(StringEnum<TEnum> lhs, TEnum rhs)
            => !lhs.Equals(rhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(TEnum lhs, StringEnum<TEnum> rhs)
            => rhs.Equals(lhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(TEnum lhs, StringEnum<TEnum> rhs)
            => !rhs.Equals(lhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(StringEnum<TEnum> lhs, StringEnum<TEnum> rhs)
            => lhs.Equals(rhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(StringEnum<TEnum> lhs, StringEnum<TEnum> rhs)
            => !lhs.Equals(rhs);

        public readonly override bool Equals(object obj)
            => obj switch {
                StringEnum<TEnum> other => Equals(other),
                TEnum other => Equals(other),
                string other => string.Equals(_value, other, StringComparison.Ordinal),
                _ => false,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(StringEnum<TEnum> other)
            => string.Equals(_value, other._value, StringComparison.Ordinal);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(TEnum other)
            => Equals((StringEnum<TEnum>)other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => HashValue.Combine(_value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly TEnum ToEnum()
            => Enum.Parse<TEnum>(_value ?? string.Empty, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => _value;

        /// <summary>
        /// Returns a <see cref="StringEnum{TEnum, TConverter}"/> with a specialized <typeparamref name="TConverter"/>.
        /// </summary>
        /// <typeparam name="TConverter">The converter to provide the specialized conversion APIs.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly StringEnum<TEnum, TConverter> WithConverter<TConverter>()
            where TConverter : struct, IStringEnumConverter<TEnum>
            => this;
    }
}
