using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules
{
    /// <summary>
    /// Represents a key which can be used to load an asset via Resources or Addressables APIs.
    /// </summary>
    /// <typeparam name="T">The type of the asset.</typeparam>
    /// <remarks>
    /// Unlike <see cref="AssetKey"/>, this struct provides additional type safety by enforcing the type of the asset.
    /// </remarks>
    public readonly partial struct AssetKey<T> : IEquatable<AssetKey<T>>, IEquatable<AssetKey>
    {
        /// <summary>
        /// The value of the key.
        /// </summary>
        private readonly AssetKey _value;

        /// <summary>
        /// Constructs a new of <see cref="AssetKey{T}"/>.
        /// </summary>
        /// <param name="value">The value of the key.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AssetKey(string value)
        {
            _value = value;
        }

        /// <summary>
        /// Constructs a new of <see cref="AssetKey{T}"/>.
        /// </summary>
        /// <param name="value">The value of the key.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AssetKey(AssetKey value)
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
            get => _value.IsValid;
        }

        /// <summary>
        /// Gets the value of the key.
        /// </summary>
        public string Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value.Value;
        }

        /// <summary>
        /// Determines whether the specified <see cref="AssetKey{T}"/> is equal to the current <see cref="AssetKey{T}"/>.
        /// </summary>
        /// <param name="other">
        /// The <see cref="AssetKey{T}"/> to compare with the current <see cref="AssetKey{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="AssetKey{T}"/> is equal to the current <see cref="AssetKey{T}"/>;
        /// </returns>
        /// <remarks>
        /// Two keys are considered equal if their values are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(AssetKey<T> other)
            => _value.Equals(other._value);

        /// <summary>
        /// Determines whether the specified <see cref="AssetKey"/> is equal to the current <see cref="AssetKey{T}"/>.
        /// </summary>
        /// <param name="other">
        /// The <see cref="AssetKey"/> to compare with the current <see cref="AssetKey{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="AssetKey"/> is equal to the current <see cref="AssetKey{T}"/>;
        /// </returns>
        /// <remarks>
        /// Two keys are considered equal if their values are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(AssetKey other)
            => _value.Equals(other);

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="AssetKey{T}"/>.
        /// </summary>
        /// <param name="obj">
        /// The object to compare with the current <see cref="AssetKey{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified object is equal to the current <see cref="AssetKey{T}"/>;
        /// </returns>
        /// <remarks>
        /// <paramref name="obj"/> is considered equal to the current <see cref="AssetKey{T}"/>
        /// if it is an instance of either <see cref="AssetKey{T}"/> or <see cref="AssetKey"/>
        /// and their values are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is AssetKey<T> otherT
                ? _value.Equals(otherT._value)
                : obj is AssetKey other && _value.Equals(other);

        /// <summary>
        /// Returns the hash code for this <see cref="AssetKey{TagHandle}"/>.
        /// </summary>
        /// <remarks>
        /// The hash code is calculated based on the value of the key.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _value.GetHashCode();

        /// <summary>
        /// Returns a string representation of this <see cref="AssetKey{TagHandle}"/>.
        /// </summary>
        /// <remarks>
        /// The string representation is the value of the key.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _value.ToString();

        /// <summary>
        /// Converts the current <see cref="AssetKey{T}"/> to an instance of <see cref="AssetKey"/>.
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator AssetKey(AssetKey<T> value)
            => value._value;

        /// <summary>
        /// Converts the current <see cref="AssetKey{T}"/> to an instance of <see cref="string"/>.
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string(AssetKey<T> value)
            => value._value.ToString();

        /// <summary>
        /// Converts the specified <see cref="string"/> to an instance of <see cref="AssetKey{T}"/>.
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetKey<T>(string value)
            => new(value);

        /// <summary>
        /// Converts the specified <see cref="AssetKey"/> to an instance of <see cref="AssetKey{T}"/>.
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AssetKey<T>(AssetKey value)
            => new(value);

        /// <summary>
        /// Determines whether two specified instances of <see cref="AssetKey{T}"/> are equal.
        /// </summary>
        /// <param name="left">The first <see cref="AssetKey{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="AssetKey{T}"/> to compare.</param>
        /// <returns>
        /// <c>true</c> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>;
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(AssetKey<T> left, AssetKey<T> right)
            => left.Equals(right);

        /// <summary>
        /// Determines whether two specified instances of <see cref="AssetKey{T}"/> are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="AssetKey{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="AssetKey{T}"/> to compare.</param>
        /// <returns>
        /// <c>true</c> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>;
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(AssetKey<T> left, AssetKey<T> right)
            => !left.Equals(right);
    }

    partial struct AssetKey
    {
        /// <summary>
        /// Represents a serializable version of <see cref="AssetKey{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Serializable]
        public partial struct Serializable<T> : ITryConvert<AssetKey<T>>
            , IEquatable<Serializable<T>>
        {
            /// <summary>
            /// The value of the key.
            /// </summary>
            [SerializeField]
            private string _value;

            /// <summary>
            /// Constructs a new of <see cref="Serializable{T}"/>.
            /// </summary>
            /// <param name="value"></param>
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
            /// Attempts to convert the serializable key to an <see cref="AssetKey{T}"/>.
            /// </summary>
            /// <param name="result">
            /// When this method returns, contains the <see cref="AssetKey{T}"/> equivalent of the serializable key.
            /// </param>
            /// <returns>
            /// <c>true</c> if the conversion was successful; otherwise, <c>false</c>.
            /// </returns>
            /// <remarks>
            /// The conversion is always successful.
            /// </remarks>
            public readonly bool TryConvert(out AssetKey<T> result)
            {
                result = new(_value);
                return true;
            }

            /// <summary>
            /// Determines whether the specified <see cref="Serializable{T}"/> is equal to the current <see cref="Serializable{T}"/>.
            /// </summary>
            /// <param name="other">
            /// The <see cref="Serializable{T}"/> to compare with the current <see cref="Serializable{T}"/>.
            /// </param>
            /// <returns>
            /// <c>true</c> if the specified <see cref="Serializable{T}"/> is equal to the current <see cref="Serializable{T}"/>;
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool Equals(Serializable<T> other)
                => string.Equals(_value, other._value, StringComparison.Ordinal);

            /// <summary>
            /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="Serializable{T}"/>.
            /// </summary>
            /// <param name="obj">
            /// The <see cref="object"/> to compare with the current <see cref="Serializable{T}"/>.
            /// </param>
            /// <returns>
            /// <c>true</c> if the specified <see cref="object"/> is equal to the current <see cref="Serializable{T}"/>;
            /// </returns>
            /// <remarks>
            /// <paramref name="obj"/> is considered equal to the current <see cref="Serializable{T}"/>
            /// if it is a <see cref="Serializable{T}"/> and their values are equal.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly bool Equals(object obj)
                => obj is Serializable<T> other && Equals(other);

            /// <summary>
            /// Returns the hash code for this <see cref="Serializable{T}"/>.
            /// </summary>
            /// <returns>
            /// The hash code for this <see cref="Serializable{T}"/>.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override readonly int GetHashCode()
                => _value.GetHashCode(StringComparison.Ordinal);

            /// <summary>
            /// Returns a string representation of this <see cref="Serializable{T}"/>.
            /// </summary>
            /// <returns>
            /// The value of the key.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString()
                => _value ?? string.Empty;

            /// <summary>
            /// Implicitly converts the specified <see cref="Serializable{T}"/> to a <see cref="string"/>.
            /// </summary>
            /// <param name="value">
            /// The <see cref="Serializable{T}"/> to convert.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static explicit operator string(Serializable<T> value)
                => value._value;

            /// <summary>
            /// Implicitly converts the specified <see cref="string"/> to a <see cref="Serializable{T}"/>.
            /// </summary>
            /// <param name="value">
            /// The <see cref="string"/> to convert.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable<T>(string value)
                => new(value);

            /// <summary>
            /// Converts the specified <see cref="Serializable"/> to a <see cref="Serializable{T}"/>.
            /// </summary>
            /// <param name="value">
            /// The <see cref="Serializable"/> to convert.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable<T>(Serializable value)
                => new(value.Value);

            /// <summary>
            /// Converts the specified <see cref="Serializable{T}"/> to an <see cref="AssetKey{T}"/>.
            /// </summary>
            /// <param name="value">
            /// The <see cref="Serializable{T}"/> to convert.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator AssetKey<T>(Serializable<T> value)
                => new(value._value);

            /// <summary>
            /// Converts the specified <see cref="AssetKey{T}"/> to a <see cref="Serializable{T}"/>.
            /// </summary>
            /// <param name="value">
            /// The <see cref="AssetKey{T}"/> to convert.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Serializable<T>(AssetKey<T> value)
                => new((string)value);

            /// <summary>
            /// Determines whether two specified <see cref="Serializable{T}"/> objects have the same value.
            /// </summary>
            /// <param name="left">The first <see cref="AssetKey{T}"/> to compare.</param>
            /// <param name="right">The second <see cref="AssetKey{T}"/> to compare.</param>
            /// <returns>
            /// <c>true</c> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>;
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Serializable<T> left, Serializable<T> right)
                => left._value == right._value;

            /// <summary>
            /// Determines whether two specified <see cref="Serializable{T}"/> objects have different values.
            /// </summary>
            /// <param name="left">The first <see cref="AssetKey{T}"/> to compare.</param>
            /// <param name="right">The second <see cref="AssetKey{T}"/> to compare.</param>
            /// <returns>
            /// <c>true</c> if the value of <paramref name="left"/> is different from the value of <paramref name="right"/>;
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Serializable<T> left, Serializable<T> right)
                => left._value != right._value;
        }
    }
}
