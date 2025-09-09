using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Conversion;
using UnityEngine;

namespace EncosyTower.ConfigKeys
{
    /// <summary>
    /// Represents a key which can be used to load a configuration value.
    /// </summary>
    public readonly partial struct ConfigKey : IEquatable<ConfigKey>
    {
        /// <summary>
        /// The value of the key.
        /// </summary>
        private readonly string _value;

        /// <summary>
        /// Constructs a new of <see cref="ConfigKey"/>.
        /// </summary>
        /// <param name="value">The value of the key.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConfigKey(string value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets a boolean value indicating whether the key is valid.
        /// </summary>
        /// <remarks>
        /// A key is considered valid if it is not null or empty.
        /// </remarks>
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value.IsNotEmpty();
        }

        /// <summary>
        /// Gets the value of the key.
        /// </summary>
        public string Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ConfigKey"/> is equal to the current <see cref="ConfigKey"/>.
        /// </summary>
        /// <param name="other">
        /// The <see cref="ConfigKey"/> to compare with the current <see cref="ConfigKey"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="ConfigKey"/> is equal to the current <see cref="ConfigKey"/>;
        /// </returns>
        /// <remarks>
        /// Two keys are considered equal if their values are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ConfigKey other)
            => string.Equals(_value, other._value, StringComparison.Ordinal);

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="ConfigKey"/>.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="object"/> to compare with the current <see cref="ConfigKey"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="object"/> is equal to the current <see cref="ConfigKey"/>;
        /// </returns>
        /// <remarks>
        /// <paramref name="obj"/> is considered equal to the current <see cref="ConfigKey"/>
        /// if it is an <see cref="ConfigKey"/> and their values are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is ConfigKey other && Equals(other);

        /// <summary>
        /// Returns the hash code for this <see cref="ConfigKey"/>.
        /// </summary>
        /// <remarks>
        /// The hash code is calculated based on the value of the key.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(_value);

        /// <summary>
        /// Returns a string representation of this <see cref="ConfigKey"/>.
        /// </summary>
        /// <remarks>
        /// The string representation is the value of the key.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _value ?? string.Empty;

        /// <summary>
        /// Converts the specified <see cref="ConfigKey"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="value">
        /// The <see cref="ConfigKey"/> to convert.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string(ConfigKey value)
            => value._value;

        /// <summary>
        /// Converts the specified <see cref="string"/> to an <see cref="ConfigKey"/>.
        /// </summary>
        /// <param name="value">
        /// The <see cref="string"/> to convert.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ConfigKey(string value)
            => new(value);

        /// <summary>
        /// Determines whether two specified <see cref="ConfigKey"/> objects have the same value.
        /// </summary>
        /// <param name="left">The first <see cref="ConfigKey"/> to compare.</param>
        /// <param name="right">The second <see cref="ConfigKey"/> to compare.</param>
        /// <returns>
        /// <c>true</c> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>;
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ConfigKey left, ConfigKey right)
            => left.Equals(right);

        /// <summary>
        /// Determines whether two specified <see cref="ConfigKey"/> objects have different values.
        /// </summary>
        /// <param name="left">The first <see cref="ConfigKey"/> to compare.</param>
        /// <param name="right">The second <see cref="ConfigKey"/> to compare.</param>
        /// <returns>
        /// <c>true</c> if the value of <paramref name="left"/> is different from the value of <paramref name="right"/>;
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ConfigKey left, ConfigKey right)
            => !left.Equals(right);

        /// <summary>
        /// Represents a serializable version of <see cref="ConfigKey"/>.
        /// </summary>
        [Serializable]
        public partial struct Serializable : ITryConvert<ConfigKey>
            , IEquatable<Serializable>
        {
            /// <summary>
            /// The value of the key.
            /// </summary>
            [SerializeField]
            private string _value;

            /// <summary>
            /// Constructs a new of <see cref="Serializable"/>.
            /// </summary>
            /// <param name="value">
            /// The value of the key.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Serializable(string value)
            {
                _value = value;
            }

            /// <summary>
            /// Gets a boolean value indicating whether the key is valid.
            /// </summary>
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
            /// Attempts to convert the serializable key to an <see cref="ConfigKey"/>.
            /// </summary>
            /// <param name="result">
            /// When this method returns, contains the <see cref="ConfigKey"/> equivalent of the serializable key.
            /// </param>
            /// <returns>
            /// <c>true</c> if the conversion was successful; otherwise, <c>false</c>.
            /// </returns>
            /// <remarks>
            /// The conversion is always successful.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryConvert(out ConfigKey result)
            {
                result = new(_value);
                return true;
            }

            /// <summary>
            /// Determines whether the specified <see cref="Serializable"/> is equal to the current <see cref="Serializable"/>.
            /// </summary>
            /// <param name="other">
            /// The <see cref="Serializable"/> to compare with the current <see cref="Serializable"/>.
            /// </param>
            /// <returns>
            /// <c>true</c> if the specified <see cref="Serializable"/> is equal to the current <see cref="Serializable"/>;
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Equals(Serializable other)
                => string.Equals(_value, other._value, StringComparison.Ordinal);

            /// <summary>
            /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="Serializable"/>.
            /// </summary>
            /// <param name="obj">
            /// The <see cref="object"/> to compare with the current <see cref="Serializable"/>.
            /// </param>
            /// <returns>
            /// <c>true</c> if the specified <see cref="object"/> is equal to the current <see cref="Serializable"/>;
            /// </returns>
            /// <remarks>
            /// <paramref name="obj"/> is considered equal to the current <see cref="Serializable"/>
            /// if it is a <see cref="Serializable"/> and their values are equal.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override bool Equals(object obj)
                => obj is Serializable other && Equals(other);

            /// <summary>
            /// Returns the hash code for this <see cref="Serializable"/>.
            /// </summary>
            /// <returns>
            /// The hash code for this <see cref="Serializable"/>.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override int GetHashCode()
                => _value.GetHashCode(StringComparison.Ordinal);

            /// <summary>
            /// Returns a string representation of this <see cref="Serializable"/>.
            /// </summary>
            /// <returns>
            /// The value of the key.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly override string ToString()
                => _value ?? string.Empty;

            /// <summary>
            /// Implicitly converts the specified <see cref="Serializable"/> to a <see cref="string"/>.
            /// </summary>
            /// <param name="value">
            /// The <see cref="Serializable"/> to convert.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static explicit operator string(Serializable value)
                => value._value;

            /// <summary>
            /// Implicitly converts the specified <see cref="string"/> to a <see cref="Serializable"/>.
            /// </summary>
            /// <param name="value">
            /// The <see cref="string"/> to convert.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(string value)
                => new(value);

            /// <summary>
            /// Converts the specified <see cref="Serializable"/> to an <see cref="ConfigKey"/>.
            /// </summary>
            /// <param name="value">
            /// The <see cref="Serializable"/> to convert.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ConfigKey(Serializable value)
                => new(value._value);

            /// <summary>
            /// Converts the specified <see cref="ConfigKey"/> to a <see cref="Serializable"/>.
            /// </summary>
            /// <param name="value">
            /// The <see cref="ConfigKey"/> to convert.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(ConfigKey value)
                => new(value._value);

            /// <summary>
            /// Determines whether two specified <see cref="Serializable"/> objects have the same value.
            /// </summary>
            /// <param name="left">The first <see cref="ConfigKey"/> to compare.</param>
            /// <param name="right">The second <see cref="ConfigKey"/> to compare.</param>
            /// <returns>
            /// <c>true</c> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>;
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable left, Serializable right)
                => string.Equals(left._value, right._value, StringComparison.Ordinal);

            /// <summary>
            /// Determines whether two specified <see cref="Serializable"/> objects have different values.
            /// </summary>
            /// <param name="left">The first <see cref="ConfigKey"/> to compare.</param>
            /// <param name="right">The second <see cref="ConfigKey"/> to compare.</param>
            /// <returns>
            /// <c>true</c> if the value of <paramref name="left"/> is different from the value of <paramref name="right"/>;
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable left, Serializable right)
                => !string.Equals(left._value, right._value, StringComparison.Ordinal);
        }
    }
}
