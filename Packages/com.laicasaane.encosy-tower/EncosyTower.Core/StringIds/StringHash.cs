using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.StringIds
{
    /// <summary>
    /// Represents the hash value of a string using <see cref="HashValue64"/>.
    /// </summary>
    internal readonly partial record struct StringHash(ulong Value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringHash(ulong value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ulong(StringHash hash)
            => hash.Value;
    }
}
