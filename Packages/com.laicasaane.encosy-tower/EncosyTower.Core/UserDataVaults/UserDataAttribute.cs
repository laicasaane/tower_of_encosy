#if UNITASK || UNITY_6000_0_OR_NEWER

using System;

namespace EncosyTower.UserDataVaults
{
    /// <summary>
    /// Applying <see cref="UserDataAttribute"/> to a class or struct to generate
    /// implementation for <see cref="IUserData"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class UserDataAttribute : Attribute
    {
    }
}

#endif
