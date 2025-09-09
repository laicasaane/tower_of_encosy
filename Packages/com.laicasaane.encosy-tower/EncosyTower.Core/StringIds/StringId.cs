using System.Runtime.CompilerServices;
using EncosyTower.Ids;
using EncosyTower.TypeWraps;

namespace EncosyTower.StringIds
{
    /// <summary>
    /// Represents a lightweight handle for a string.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="StringId"/> must be retrieved from <see cref="StringToId"/>.
    /// <br/>
    /// <see cref="StringId"/> constructors should not be used because they cannot
    /// guarantee the uniqueness of a string or its (assumingly) associative ID.
    /// </remarks>
    [WrapRecord]
    public readonly partial record struct StringId(Id Id)
    {
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Id > 0;
        }
    }

    /// <summary>
    /// Represents a lightweight typed handle for a string.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="StringId"/> must be retrieved from <see cref="StringToId"/>.
    /// <br/>
    /// <see cref="StringId"/> constructors should not be used because they cannot
    /// guarantee the uniqueness of a string or its (assumingly) associative ID.
    /// </remarks>
    [WrapRecord]
    public readonly partial record struct StringId<T>(Id Id)
    {
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Id > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringId<T>(StringId id)
            => new(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringId(StringId<T> id)
            => new(id);
    }

    /// <summary>
    /// Represents the hash value of a string.
    /// </summary>
    internal readonly partial record struct StringHash(int Value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringHash(int value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(StringHash hash)
            => hash.Value;
    }
}
