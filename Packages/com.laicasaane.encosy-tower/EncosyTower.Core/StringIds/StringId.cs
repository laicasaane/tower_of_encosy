using System.Runtime.CompilerServices;
using EncosyTower.Ids;
using EncosyTower.TypeWraps;

namespace EncosyTower.StringIds
{
    /// <summary>
    /// Represents a lightweight handle for a string. <see cref="StringId"/> must be retrieved from either:
    /// <list type="bullet">
    /// <item>The global API <see cref="StringToId"/></item>
    /// <item>A local <see cref="StringVault"/></item>
    /// <item>A local <see cref="NativeStringVault"/></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// <see cref="StringId"/> constructor should not be directly used by user code
    /// because it does not guarantee the uniqueness of a string or the associative <see cref="Id"/>.
    /// </remarks>
    [WrapRecord]
    public readonly partial record struct StringId(Id Id)
    {
        /// <summary>
        /// A <see cref="StringId"/> is invalid when it is registered within a string vault.
        /// </summary>
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Id > 0;
        }
    }

    /// <summary>
    /// Represents a lightweight handle for a string. <see cref="StringId{T}"/> must be retrieved from either:
    /// <list type="bullet">
    /// <item>The global API <see cref="StringToId"/></item>
    /// <item>A local <see cref="StringVault"/></item>
    /// <item>A local <see cref="NativeStringVault"/></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// <see cref="StringId{T}"/> constructor should not be directly used by user code
    /// because it does not guarantee the uniqueness of a string or the associative <see cref="Id"/>.
    /// </remarks>
    [WrapRecord]
    public readonly partial record struct StringId<T>(Id<T> Id)
    {
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Id > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringId<T>(StringId id)
            => new(id.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringId(StringId<T> id)
            => new(id.Id);
    }
}
