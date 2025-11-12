#if UNITY_COLLECTIONS

using EncosyTower.Common;
using EncosyTower.TypeWraps;

namespace EncosyTower.StringIds
{
    /// <summary>
    /// Represents the hash value of a string using <see cref="HashValue64"/>.
    /// </summary>
    [WrapRecord]
    internal readonly partial record struct StringHash(ulong HashCode64);
}

#endif
