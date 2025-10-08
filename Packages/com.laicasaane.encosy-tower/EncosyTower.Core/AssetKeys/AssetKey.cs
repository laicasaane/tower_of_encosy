using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Conversion;
using EncosyTower.Serialization;
using UnityEngine;

namespace EncosyTower.AssetKeys
{
    /// <summary>
    /// Represents a key which can be used to load an asset via various asset management systems.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(TypeConverter))]
    public partial struct AssetKey : IEquatable<AssetKey>, ITryParse<AssetKey>
    {
        /// <summary>
        /// The value of the key.
        /// </summary>
        [SerializeField] internal string _value;

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
        public readonly bool Equals(AssetKey other)
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
        public readonly override bool Equals(object obj)
            => obj is AssetKey other && Equals(other);

        /// <summary>
        /// Returns the hash code for this <see cref="AssetKey"/>.
        /// </summary>
        /// <remarks>
        /// The hash code is calculated based on the value of the key.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => _value?.GetHashCode() ?? 0;

        /// <summary>
        /// Returns a string representation of this <see cref="AssetKey"/>.
        /// </summary>
        /// <remarks>
        /// The string representation is the value of the key.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => _value ?? string.Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryParse(
              string str
            , out AssetKey result
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

        public sealed class TypeConverter : ParsableStructConverter<AssetKey>
        {
            public override bool IgnoreCase => false;

            public override bool AllowMatchingMetadataAttribute => false;
        }
    }
}
