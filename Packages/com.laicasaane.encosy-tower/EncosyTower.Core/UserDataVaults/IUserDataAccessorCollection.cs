#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Initialization;

namespace EncosyTower.UserDataVaults
{
    public interface IUserDataAccessorCollection : IUserDataAccessorReadOnlyCollection
        , IInitializable, IDeinitializable
    {
    }

    public interface IUserDataAccessorReadOnlyCollection
        : IReadOnlyList<IUserDataAccessor>, ICopyToSpan<IUserDataAccessor>, ITryCopyToSpan<IUserDataAccessor>
    {
    }
}

#endif
