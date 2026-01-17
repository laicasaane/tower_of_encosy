#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Encryption;
using EncosyTower.Initialization;
using EncosyTower.Logging;
using EncosyTower.StringIds;

namespace EncosyTower.UserDataVaults
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public abstract class UserDataSourceBase<TData> : IInitializable
        where TData : IUserData
    {
        protected UserDataSourceBase(
              StringId<string> key
            , [NotNull] StringVault stringVault
            , [NotNull] EncryptionBase encryption
            , ILogger logger
            , bool ignoreEncryption
            , UserDataSourceArgs _
        )
        {
            Key = key;
            StringVault = stringVault;
            Encryption = encryption;
            Logger = logger ?? DevLogger.Default;
            IgnoreEncryption = ignoreEncryption;
        }

        public StringId<string> Key { get; }

        public StringVault StringVault { get; }

        public EncryptionBase Encryption { get; }

        public bool IgnoreEncryption
        {
#if ENFORCE_USER_DATA_ENCRYPTION
            get => false;
            set { }
#else
            get;
#endif
        }

        public ILogger Logger { get; }

        public bool IsDirty { get; set; }

        public virtual void Initialize() { }

        public async UnityTask SaveAsync([NotNull] TData data, CancellationToken token)
        {
            if (IsDirty == false)
            {
                return;
            }

            IsDirty = false;

            await OnSaveAsync(data, token);
        }

        protected abstract UnityTask OnSaveAsync([NotNull] TData data, CancellationToken token);

        public abstract
#if UNITASK
            Cysharp.Threading.Tasks.UniTask
#else
            UnityEngine.Awaitable
#endif
            <Option<TData>> TryLoadAsync(CancellationToken token);

        public abstract Option<TData> TryCloneData(TData source);
    }
}

#endif
