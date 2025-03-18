using System.Diagnostics.CodeAnalysis;
using EncosyTower.Encryption;
using EncosyTower.StringIds;

namespace EncosyTower.UserDataVaults
{
#if UNITASK
    using UnityTask = global::Cysharp.Threading.Tasks.UniTask;
#elif UNITY_6000_0_OR_NEWER
    using UnityTask = global::UnityEngine.Awaitable;
#else
    using UnityTask = global::System.Threading.Tasks.ValueTask;
#endif

    public abstract class UserDataStorageBase<T> where T : IUserData, new()
    {
        protected UserDataStorageBase(StringId<string> key, [NotNull] EncryptionBase encryption)
        {
            Key = key;
            Encryption = encryption;
        }

        public StringId<string> Key { get; }

        public EncryptionBase Encryption { get; }

        public abstract string UserId { get; set; }

        public abstract T Data { get; }

        public bool IsDataValid => Data != null;

        public abstract void Initialize();

        public abstract void SetUserData(string userId, string version);

        public abstract void MarkDirty(bool isDirty = true);

        public abstract void CreateData();

        public abstract UnityTask LoadFromDeviceAsync();

        public abstract UnityTask LoadFromCloudAsync();

        public abstract T GetDataFromDevice();

        public abstract T GetDataFromCloud();

        public abstract void SetToDevice(T data);

        public abstract void SaveToDevice();

        public abstract UnityTask SaveToCloudAsync();

        public abstract UnityTask SaveAsync(bool forceToCloud);

        public abstract void Save(bool forceToCloud);

        public abstract void DeepCloneDataFromCloudToDevice();
    }
}
