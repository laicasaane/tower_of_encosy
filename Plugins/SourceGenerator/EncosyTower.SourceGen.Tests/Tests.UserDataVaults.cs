using System;
using System.ComponentModel;
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

    [DisplayName("Common")]
    [UserDataAccessor(typeof(UserCommonVault))]
    public sealed class UserCommonDataAccessor : IUserDataAccessor, IInitializable, IDeinitializable
    {
        internal UserCommonDataAccessor(UserDataStoreDefault<UserCommonData> storage) { }

        public void Deinitialize()
        {
        }

        public void Initialize()
        {
        }
    }

    [DisplayName("Progress")]
    [UserDataAccessor(typeof(UserProgressVault))]
    public sealed class UserProgressDataAccessor : IUserDataAccessor, IInitializable, IDeinitializable
    {
        internal UserProgressDataAccessor(UserDataStoreDefault<UserProgressData> storage) { }

        public void Deinitialize()
        {
        }

        public void Initialize()
        {
        }
    }

    [DisplayName("Event")]
    [UserDataAccessor(typeof(UserProgressVault))]
    public sealed class UserEventDataAccessor : IUserDataAccessor, IInitializable, IDeinitializable
    {
        internal UserEventDataAccessor(UserDataStoreDefault<UserEventData> storage) { }

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
        partial class DataDirectory
        {
            static partial void GetIgnoreEncryption(ref bool ignoreEncryption)
            {
            }

            private static partial UserDataStoreArgs GetStoreArgs<TData, TStore>(Func<TData> createDataFunc)
                where TData : IUserData
                where TStore : UserDataStoreBase<TData>
            {
                return default;
            }
        }
    }

    [UserDataVault]
    public static partial class UserProgressVault
    {
        partial class DataDirectory
        {
            static partial void GetIgnoreEncryption(ref bool ignoreEncryption)
            {
            }

            private static partial UserDataStoreArgs GetStoreArgs<TData, TStore>(Func<TData> createDataFunc)
                where TData : IUserData
                where TStore : UserDataStoreBase<TData>
            {
                return default;
            }
        }
    }
}
