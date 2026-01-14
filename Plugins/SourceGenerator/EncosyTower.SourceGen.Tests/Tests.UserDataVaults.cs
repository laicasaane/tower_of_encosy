using System;
using System.Diagnostics.CodeAnalysis;
using EncosyTower.Initialization;
using EncosyTower.UserDataVaults;

namespace EncosyTower.Tests.UserDataVaults
{
    internal partial class UserCommonData : IUserData
    {
        public string Id { get; set; }

        public string Version { get; set; }
    }

    internal partial class UserEventData : IUserData
    {
        public string Id { get; set; }

        public string Version { get; set; }
    }

    public sealed class UserCommonDataAccess : IUserDataAccess, IInitializable, IDeinitializable
    {
        internal UserCommonDataAccess(UserDataStorageDefault<UserCommonData> storage) { }

        public void Deinitialize()
        {
        }

        public void Initialize()
        {
        }
    }

    public sealed class UserEventDataAccess : IUserDataAccess, IInitializable, IDeinitializable
    {
        internal UserEventDataAccess(UserDataStorageDefault<UserEventData> storage) { }

        public void Deinitialize()
        {
        }

        public void Initialize()
        {
        }
    }

    [UserDataVault]
    internal static partial class UserDataVault
    {
        partial class DataStorage
        {
            static partial void GetIgnoreEncryption(ref bool ignoreEncryption)
            {
            }

            static partial void GetArgsForStorage<TData, TStorage>(ref UserDataStorageArgs storageArgs)
                where TData : IUserData
                where TStorage : UserDataStorageBase<TData>
            {
            }
        }

        partial class Collection
        {
        }
    }
}
