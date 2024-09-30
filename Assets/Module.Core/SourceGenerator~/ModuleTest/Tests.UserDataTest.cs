using Module.Core.UserDataStores;

namespace Module.Core.Tests.UserDataTest
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
