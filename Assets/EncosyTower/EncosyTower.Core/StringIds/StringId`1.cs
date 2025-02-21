using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Ids;

namespace EncosyTower.StringIds
{
    /// <summary>
    /// Represents a lightweight handle for a string.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="StringId{T}"/> must be retrieved from static APIs, such as
    /// <see cref="FixedStringToId"/> or <see cref="StringToId"/>.
    /// <br/>
    /// <see cref="StringId{T}"/> constructors should not be used because they cannot
    /// guarantee the uniqueness of a string or its (assumingly) associative ID.
    /// </remarks>
    public readonly struct StringId<T> : IEquatable<StringId<T>>
    {
        /// <summary>
        /// The unique identifier associates to a string.
        /// </summary>
        /// <remarks>
        /// To retrieve the actual string using this ID,
        /// use either <see cref="IdToFixedString"/> or <see cref="IdToString"/>
        /// depends on which one has been previously used to retrieve the ID.
        /// </remarks>
        public readonly Id<T> Id;

        /// <summary>
        /// Whether this identifier has been retrieved from
        /// <see cref="FixedStringToId"/> or <see cref="StringToId"/>.
        /// </summary>
        public readonly Bool<T> IsFixedString;

        [Obsolete("StringId should be retrieved from FixedStringToId or StringToId APIs.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringId(Id id, Bool<T> isFixedString)
        {
            Id = id;
            IsFixedString = isFixedString;
        }

        [Obsolete("StringId should be retrieved from FixedStringToId or StringToId APIs.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringId(Id<T> id, Bool<T> isFixedString)
        {
            Id = id;
            IsFixedString = isFixedString;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(StringId<T> other)
            => Id == other.Id && IsFixedString == other.IsFixedString;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is StringId<T> other && Id == other.Id && IsFixedString == other.IsFixedString;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(Id.GetHashCode(), IsFixedString.GetHashCode());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in StringId<T> left, in StringId<T> right)
            => left.Id == right.Id && left.IsFixedString == right.IsFixedString;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in StringId<T> left, in StringId<T> right)
            => left.Id != right.Id || left.IsFixedString != right.IsFixedString;
    }
}
