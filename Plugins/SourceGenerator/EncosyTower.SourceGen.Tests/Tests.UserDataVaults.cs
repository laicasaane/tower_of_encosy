using EncosyTower.Initialization;
using EncosyTower.UserDataVaults;

namespace EncosyTower.Tests.UserDataVaults
{
    internal partial class UserCommonData : IUserData
    {
        public string Id { get; set; }

        public string Version { get; set; }
    }

    internal partial class UserProgressData : IUserData
    {
        public string Id { get; set; }

        public string Version { get; set; }
    }

    internal partial class UserEventData : IUserData
    {
        public string Id { get; set; }

        public string Version { get; set; }
    }

    [UserDataAccess(typeof(UserCommonVault))]
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

    [UserDataAccess(typeof(UserProgressVault))]
    public sealed class UserProgressDataAccess : IUserDataAccess, IInitializable, IDeinitializable
    {
        internal UserProgressDataAccess(UserDataStorageDefault<UserProgressData> storage) { }

        public void Deinitialize()
        {
        }

        public void Initialize()
        {
        }
    }

    [UserDataAccess(typeof(UserProgressVault))]
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
    internal static partial class UserCommonVault
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

    [UserDataVault]
    internal static partial class UserProgressVault
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
