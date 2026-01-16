#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Collections.Generic;
using EncosyTower.Collections;

namespace EncosyTower.UserDataVaults
{
    public interface IUserDataCollection : IReadOnlyList<IUserData>, ICopyToSpan<IUserData>, ITryCopyToSpan<IUserData>
    {
    }
}

#endif
