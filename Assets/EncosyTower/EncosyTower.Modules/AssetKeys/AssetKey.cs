using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules
{
    /// <summary>
    /// Represents a key which can be used to load an asset via various asset management systems.
    /// </summary>
    public readonly partial struct AssetKey : IEquatable<AssetKey>
    {
        /// <summary>
        /// The value of the key.
        /// </summary>
        private readonly string _value;

        /// <summary>
        /// Constructs a new of <see cref="AssetKey"/>.
        /// </summary>
        /// <param name="value">The value of the key.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AssetKey(string value)
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
            get => string.IsNullOrEmpty(_value) == false;
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
        /// Determines whether the specified <see cref="AssetKey"/> is equal to the current <see cref="AssetKey"/>.
        /// </summary>
        /// <param name="other">
        /// The <see cref="AssetKey"/> to compare with the current <see cref="AssetKey"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="AssetKey"/> is equal to the current <see cref="AssetKey"/>;
        /// </returns>
        /// <remarks>
        /// Two keys are considered equal if their values are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(AssetKey other)
            => string.Equals(_value, other._value, StringComparison.Ordinal);

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="AssetKey"/>.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="object"/> to compare with the current <see cref="AssetKey"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="object"/> is equal to the current <see cref="AssetKey"/>;
        /// </returns>
        /// <remarks>
        /// <paramref name="obj"/> is considered equal to the current <see cref="AssetKey"/>
        /// if it is an <see cref="AssetKey"/> and their values are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is AssetKey other && Equals(other);

        /// <summary>
        /// Returns the hash code for this <see cref="AssetKey"/>.
        /// </summary>
        /// <remarks>
        /// The hash code is calculated based on the value of the key.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(_value);

        /// <summary>
        /// Returns a string representation of this <see cref="AssetKey"/>.
        /// </summary>
        /// <remarks>
        /// The string representation is the value of the key.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _value ?? string.Empty;

        /// <summary>
        /// Converts the specified <see cref="AssetKey"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="value">
        /// The <see cref="AssetKey"/> to convert.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string(AssetKey value)
            => value._value;

        /// <summary>
        /// Converts the specified <see cref="string"/> to an <see cref="AssetKey"/>.
        /// </summary>
        /// <param name="value">
        /// The <see cref="string"/> to convert.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetKey(string value)
            => new(value);

        /// <summary>
        /// Determines whether two specified <see cref="AssetKey"/> objects have the same value.
        /// </summary>
        /// <param name="left">The first <see cref="AssetKey"/> to compare.</param>
        /// <param name="right">The second <see cref="AssetKey"/> to compare.</param>
        /// <returns>
        /// <c>true</c> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>;
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(AssetKey left, AssetKey right)
            => left.Equals(right);

        /// <summary>
        /// Determines whether two specified <see cref="AssetKey"/> objects have different values.
        /// </summary>
        /// <param name="left">The first <see cref="AssetKey"/> to compare.</param>
        /// <param name="right">The second <see cref="AssetKey"/> to compare.</param>
        /// <returns>
        /// <c>true</c> if the value of <paramref name="left"/> is different from the value of <paramref name="right"/>;
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(AssetKey left, AssetKey right)
            => !left.Equals(right);

        /// <summary>
        /// Represents a serializable version of <see cref="AssetKey"/>.
        /// </summary>
        [Serializable]
        public partial struct Serializable : ITryConvert<AssetKey>
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
            public bool IsValid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => string.IsNullOrEmpty(_value) == false;
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
            /// Attempts to convert the serializable key to an <see cref="AssetKey"/>.
            /// </summary>
            /// <param name="result">
            /// When this method returns, contains the <see cref="AssetKey"/> equivalent of the serializable key.
            /// </param>
            /// <returns>
            /// <c>true</c> if the conversion was successful; otherwise, <c>false</c>.
            /// </returns>
            /// <remarks>
            /// The conversion is always successful.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryConvert(out AssetKey result)
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
            public override readonly bool Equals(object obj)
                => obj is Serializable other && Equals(other);

            /// <summary>
            /// Returns the hash code for this <see cref="Serializable"/>.
            /// </summary>
            /// <returns>
            /// The hash code for this <see cref="Serializable"/>.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly int GetHashCode()
                => _value.GetHashCode(StringComparison.Ordinal);

            /// <summary>
            /// Returns a string representation of this <see cref="Serializable"/>.
            /// </summary>
            /// <returns>
            /// The value of the key.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString()
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
            /// Converts the specified <see cref="Serializable"/> to an <see cref="AssetKey"/>.
            /// </summary>
            /// <param name="value">
            /// The <see cref="Serializable"/> to convert.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator AssetKey(Serializable value)
                => new(value._value);

            /// <summary>
            /// Converts the specified <see cref="AssetKey"/> to a <see cref="Serializable"/>.
            /// </summary>
            /// <param name="value">
            /// The <see cref="AssetKey"/> to convert.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable(AssetKey value)
                => new(value._value);

            /// <summary>
            /// Determines whether two specified <see cref="Serializable"/> objects have the same value.
            /// </summary>
            /// <param name="left">The first <see cref="AssetKey"/> to compare.</param>
            /// <param name="right">The second <see cref="AssetKey"/> to compare.</param>
            /// <returns>
            /// <c>true</c> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>;
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable left, Serializable right)
                => string.Equals(left._value, right._value, StringComparison.Ordinal);

            /// <summary>
            /// Determines whether two specified <see cref="Serializable"/> objects have different values.
            /// </summary>
            /// <param name="left">The first <see cref="AssetKey"/> to compare.</param>
            /// <param name="right">The second <see cref="AssetKey"/> to compare.</param>
            /// <returns>
            /// <c>true</c> if the value of <paramref name="left"/> is different from the value of <paramref name="right"/>;
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable left, Serializable right)
                => !string.Equals(left._value, right._value, StringComparison.Ordinal);
        }
    }
}
