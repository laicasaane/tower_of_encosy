using EncosyTower.Modules.UserDataStores;

namespace EncosyTower.Modules.Tests.UserDataTest
{
    internal sealed partial class UserCommonData : UserDataBase
    {
    }

    public sealed class UserCommonDataAccess : IUserDataAccess
    {
        internal UserCommonDataAccess(UserDataStorage<UserCommonData> storage) { }
    }

    [UserDataAccessProvider]
    internal static partial class UserDataAccessVault
    {
    }
}
