#if UNITASK || UNITY_6000_0_OR_NEWER

using System;

namespace EncosyTower.UserDataVaults
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class UserDataAccessAttribute : Attribute
    {
        public UserDataAccessAttribute(Type vaultType)
        {
            VaultType = vaultType;
        }

        public Type VaultType { get; }
    }
}

#endif
