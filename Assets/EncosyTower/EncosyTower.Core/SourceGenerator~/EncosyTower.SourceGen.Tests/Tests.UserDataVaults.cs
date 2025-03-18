using Cysharp.Threading.Tasks;
using EncosyTower.Encryption;
using EncosyTower.Logging;
using EncosyTower.StringIds;
using EncosyTower.UserDataVaults;

namespace EncosyTower.Tests.UserDataVaults
{
    internal partial class UserCommonData : IUserData
    {
        public string Id { get; set; }

        public string Version { get; set; }
    }

    public sealed class UserCommonDataAccess : IUserDataAccess
    {
        internal UserCommonDataAccess(UserDataStorage<UserCommonData> storage) { }
    }

    [UserDataVault]
    internal static partial class UserDataVault
    {
    }

    public sealed class UserDataStorage<T> : UserDataStorageBase<T>
        where T : IUserData, new()
    {
        public UserDataStorage(
            StringId<string> key
            , EncryptionBase encryption
            , ILogger logger
        ) : base(key, encryption, logger)
        {
        }

        public override string UserId { get; set; }

        public override T Data => default;

        public override void CreateData() { }

        public override void DeepCloneDataFromCloudToDevice() { }

        public override T GetDataFromCloud() => default;

        public override T GetDataFromDevice() => default;

        public override void Initialize() { }

        public override UniTask LoadFromCloudAsync() => default;

        public override UniTask LoadFromDeviceAsync() => default;

        public override void MarkDirty(bool isDirty = true) { }

        public override void Save(bool forceToCloud) { }

        public override UniTask SaveAsync(bool forceToCloud) => default;

        public override UniTask SaveToCloudAsync() => default;

        public override void SaveToDevice() { }

        public override void SetToDevice(T data) { }

        public override void SetUserData(string userId, string version) { }
    }
}
