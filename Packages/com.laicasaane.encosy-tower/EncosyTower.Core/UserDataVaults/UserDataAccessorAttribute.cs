#if UNITASK || UNITY_6000_0_OR_NEWER

using System;

namespace EncosyTower.UserDataVaults
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class UserDataAccessorAttribute : Attribute
    {
        public UserDataAccessorAttribute(Type vaultType)
        {
            VaultType = vaultType;
        }

        public Type VaultType { get; }
    }
}

#endif
