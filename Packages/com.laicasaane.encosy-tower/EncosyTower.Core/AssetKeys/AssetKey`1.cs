using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Conversion;
using UnityEngine;

namespace EncosyTower.AssetKeys
{
    /// <summary>
    /// Represents a key which can be used to load an asset via Resources or Addressables APIs.
    /// </summary>
    /// <typeparam name="T">The type of the asset.</typeparam>
    /// <remarks>
    /// Unlike <see cref="AssetKey"/>, this struct provides additional type safety by enforcing the type of the asset.
    /// </remarks>
    [Serializable]
    [TypeConverter(typeof(AssetKey.TypeConverter))]
    public partial struct AssetKey<T> : IEquatable<AssetKey<T>>, IEquatable<AssetKey>, ITryParse<AssetKey<T>>
    {
        /// <summary>
        /// The value of the key.
        /// </summary>
        [SerializeField] internal string _value;

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
        public readonly bool Equals(AssetKey<T> other)
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
        public readonly bool Equals(AssetKey other)
            => _value.Equals(other._value);

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
        public override readonly bool Equals(object obj)
            => obj is AssetKey<T> otherT
                ? _value.Equals(otherT._value)
                : obj is AssetKey other && _value.Equals(other._value);

        /// <summary>
        /// Returns the hash code for this <see cref="AssetKey{TagHandle}"/>.
        /// </summary>
        /// <remarks>
        /// The hash code is calculated based on the value of the key.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode()
            => _value.GetHashCode();

        /// <summary>
        /// Returns a string representation of this <see cref="AssetKey{TagHandle}"/>.
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
            , out AssetKey<T> result
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
}
