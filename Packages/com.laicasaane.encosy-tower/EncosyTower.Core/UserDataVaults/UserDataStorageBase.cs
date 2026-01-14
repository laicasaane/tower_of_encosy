#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Encryption;
using EncosyTower.Logging;
using EncosyTower.StringIds;

namespace EncosyTower.UserDataVaults
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public abstract class UserDataStorageBase<TData>
        where TData : IUserData
    {
        protected UserDataStorageBase(
              StringId<string> key
            , [NotNull] EncryptionBase encryption
            , ILogger logger
            , bool ignoreEncryption
            , UserDataStorageArgs _
        )
        {
            Key = key;
            Encryption = encryption;
            Logger = logger ?? DevLogger.Default;
            IgnoreEncryption = ignoreEncryption;
        }

        public StringId<string> Key { get; }

        public EncryptionBase Encryption { get; }

        public ILogger Logger { get; }

        public bool IgnoreEncryption
        {
#if FORCE_USER_DATA_ENCRYPTION
            get => false;
            set { }
#else
            get;
#endif
        }

        public abstract string UserId { get; set; }

        public TData Data { get; protected set; }

        public bool IsDataValid => Data != null;

        public abstract void Initialize();

        public abstract void SetUserData(string userId, string version);

        public abstract void MarkDirty(bool isDirty = true);

        public abstract void CreateData();

        public abstract UnityTask LoadFromDeviceAsync(CancellationToken token);

        public abstract UnityTask LoadFromCloudAsync(CancellationToken token);

        public abstract TData GetDataFromDevice();

        public abstract TData GetDataFromCloud();

        public abstract void SetToDevice(TData data);

        public abstract UnityTask SaveToDeviceAsync(CancellationToken token);

        public abstract UnityTask SaveToCloudAsync(CancellationToken token);

        public abstract UnityTask SaveAsync(bool forceToCloud, CancellationToken token);

        public abstract void Save(bool forceToCloud, CancellationToken token);

        public abstract void DeepCloneDataFromCloudToDevice();
    }
}

#endif
