using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Conversion;
using UnityEngine;

namespace EncosyTower.ConfigKeys
{
    /// <summary>
    /// Represents a key which can be used to load a configuration value.
    /// </summary>
    /// <typeparam name="T">The type of the configuration value.</typeparam>
    /// <remarks>
    /// Unlike <see cref="ConfigKey"/>, this struct provides additional type safety
    /// by enforcing the type of the configuration value.
    /// </remarks>
    [Serializable]
    [TypeConverter(typeof(ConfigKey.TypeConverter))]
    public partial struct ConfigKey<T> : IEquatable<ConfigKey<T>>, IEquatable<ConfigKey>, ITryParse<ConfigKey<T>>
    {
        /// <summary>
        /// The value of the key.
        /// </summary>
        [SerializeField] private string _value;

        /// <summary>
        /// Constructs a new of <see cref="ConfigKey{T}"/>.
        /// </summary>
        /// <param name="value">The value of the key.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConfigKey(string value)
        {
            _value = value;
        }

        /// <summary>
        /// Constructs a new of <see cref="ConfigKey{T}"/>.
        /// </summary>
        /// <param name="value">The value of the key.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConfigKey(ConfigKey value)
        {
            _value = value.Value;
        }

        /// <summary>
        /// Gets a boolean value indicating whether the key is valid.
        /// </summary>
        /// <remarks>
        /// A key is considered valid if it is not null or empty.
        /// </remarks>
        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value.IsNotEmpty();
        }

        /// <summary>
        /// Gets the value of the key.
        /// </summary>
        public readonly string Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ConfigKey{T}"/> is equal to the current <see cref="ConfigKey{T}"/>.
        /// </summary>
        /// <param name="other">
        /// The <see cref="ConfigKey{T}"/> to compare with the current <see cref="ConfigKey{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="ConfigKey{T}"/> is equal to the current <see cref="ConfigKey{T}"/>;
        /// </returns>
        /// <remarks>
        /// Two keys are considered equal if their values are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ConfigKey<T> other)
            => _value.Equals(other._value);

        /// <summary>
        /// Determines whether the specified <see cref="ConfigKey"/> is equal to the current <see cref="ConfigKey{T}"/>.
        /// </summary>
        /// <param name="other">
        /// The <see cref="ConfigKey"/> to compare with the current <see cref="ConfigKey{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="ConfigKey"/> is equal to the current <see cref="ConfigKey{T}"/>;
        /// </returns>
        /// <remarks>
        /// Two keys are considered equal if their values are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ConfigKey other)
            => _value.Equals(other);

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="ConfigKey{T}"/>.
        /// </summary>
        /// <param name="obj">
        /// The object to compare with the current <see cref="ConfigKey{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified object is equal to the current <see cref="ConfigKey{T}"/>;
        /// </returns>
        /// <remarks>
        /// <paramref name="obj"/> is considered equal to the current <see cref="ConfigKey{T}"/>
        /// if it is an instance of either <see cref="ConfigKey{T}"/> or <see cref="ConfigKey"/>
        /// and their values are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly bool Equals(object obj)
            => obj is ConfigKey<T> otherT
                ? _value.Equals(otherT._value)
                : obj is ConfigKey other && _value.Equals(other);

        /// <summary>
        /// Returns the hash code for this <see cref="ConfigKey{TagHandle}"/>.
        /// </summary>
        /// <remarks>
        /// The hash code is calculated based on the value of the key.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode()
            => _value.GetHashCode();

        /// <summary>
        /// Returns a string representation of this <see cref="ConfigKey{TagHandle}"/>.
        /// </summary>
        /// <remarks>
        /// The string representation is the value of the key.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly string ToString()
            => _value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryParse(
              string str
            , out ConfigKey<T> result
            , bool ignoreCase
            , bool allowMatchingMetadataAttribute
        )
        {
            if (str.IsNotEmpty())
            {
                result = str;
                return true;
            }

            result = string.Empty;
            return false;
        }

        /// <summary>
        /// Converts the current <see cref="ConfigKey{T}"/> to an instance of <see cref="ConfigKey"/>.
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ConfigKey(ConfigKey<T> value)
            => value._value;

        /// <summary>
        /// Converts the current <see cref="ConfigKey{T}"/> to an instance of <see cref="string"/>.
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string(ConfigKey<T> value)
            => value._value.ToString();

        /// <summary>
        /// Converts the specified <see cref="string"/> to an instance of <see cref="ConfigKey{T}"/>.
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ConfigKey<T>(string value)
            => new(value);

        /// <summary>
        /// Converts the specified <see cref="ConfigKey"/> to an instance of <see cref="ConfigKey{T}"/>.
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ConfigKey<T>(ConfigKey value)
            => new(value.Value);

        /// <summary>
        /// Determines whether two specified instances of <see cref="ConfigKey{T}"/> are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ConfigKey{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="ConfigKey{T}"/> to compare.</param>
        /// <returns>
        /// <c>true</c> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>;
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ConfigKey<T> left, ConfigKey<T> right)
            => left.Equals(right);

        /// <summary>
        /// Determines whether two specified instances of <see cref="ConfigKey{T}"/> are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="ConfigKey{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="ConfigKey{T}"/> to compare.</param>
        /// <returns>
        /// <c>true</c> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>;
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ConfigKey<T> left, ConfigKey<T> right)
            => !left.Equals(right);
    }
}
