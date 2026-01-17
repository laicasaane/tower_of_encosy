#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.StringIds;

namespace EncosyTower.UserDataVaults
{
    public interface IUserDataIdCollection : IReadOnlyList<StringId<string>>
        , ICopyToSpan<StringId<string>>, ITryCopyToSpan<StringId<string>>
    {
    }
}

#endif
