#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Encryption;
using EncosyTower.Logging;
using EncosyTower.StringIds;
using EncosyTower.Tasks;

namespace EncosyTower.UserDataVaults
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public sealed class UserDataStorageDefault<TData> : UserDataStorageBase<TData>
        where TData : IUserData, new()
    {
        private readonly UserDataStoreOnDevice<TData> _store;

        public UserDataStorageDefault(
              StringId<string> key
            , [NotNull] EncryptionBase encryption
            , ILogger logger
            , bool ignoreEncryption
            , [NotNull] UserDataStorageArgs args
        )
            : base(key, encryption, logger, ignoreEncryption, args)
        {
            _store = new UserDataStoreOnDevice<TData>(
                  key
                , encryption
                , logger
                , ignoreEncryption
                , args
            );
        }

        public override string UserId
        {
            get
            {
                return Data?.Id ?? string.Empty;
            }

            set
            {
                if (Data != null)
                {
                    Data.Id = value;
                }
            }
        }

        public override void CreateData()
        {
            Data = new();
        }

        public override void DeepCloneDataFromCloudToDevice()
        {
        }

        public override TData GetDataFromCloud()
        {
            return Data;
        }

        public override TData GetDataFromDevice()
        {
            return Data;
        }

        public override void Initialize()
        {
            _store.Initialize();
        }

        public override UnityTask LoadFromCloudAsync(CancellationToken token)
            => LoadFromDeviceAsync(token);

        public override async UnityTask LoadFromDeviceAsync(CancellationToken token)
        {
            var dataOpt = await _store.TryLoadAsync(token);

            if (dataOpt.TryGetValue(out var data))
            {
                Data = data;
            }
        }

        public override void MarkDirty(bool isDirty = true)
        {
            _store.IsDirty = isDirty;
        }

        public override void Save(bool forceToCloud, CancellationToken token)
        {
            var data = Data;

            if (data != null)
            {
                UnityTasks.Forget(_store.SaveAsync(data, token));
            }
        }

        public override UnityTask SaveAsync(bool forceToCloud, CancellationToken token)
            => SaveToDeviceAsync(token);

        public override UnityTask SaveToCloudAsync(CancellationToken token)
            => SaveToDeviceAsync(token);

        public override async UnityTask SaveToDeviceAsync(CancellationToken token)
        {
            var data = Data;

            if (data != null)
            {
                await _store.SaveAsync(data, token);
            }
        }

        public override void SetToDevice(TData data)
        {
            if (data != null)
            {
                Data = data;
                _store.IsDirty = true;
            }
        }

        public override void SetUserData(string userId, string version)
        {
            var data = Data;
            data.Id = userId;
            data.Version = version;
        }
    }
}

#endif
