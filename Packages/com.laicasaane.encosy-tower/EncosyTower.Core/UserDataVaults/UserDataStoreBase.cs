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

    public abstract class UserDataStoreBase<TData> : IInitializable, IDeinitializable
        where TData : IUserData
    {
#pragma warning disable IDE0060 // Remove unused parameter
        protected UserDataStoreBase(
              StringId<string> key
            , [NotNull] StringVault stringVault
            , [NotNull] EncryptionBase encryption
            , ILogger logger
            , bool ignoreEncryption
            , UserDataStoreArgs args
        )
        {
        }
#pragma warning restore IDE0060 // Remove unused parameter

        public abstract string UserId { get; set; }

        public abstract void Initialize();

        public abstract void Deinitialize();

        public abstract void CreateData();

        public abstract void MarkDirty(bool isDirty = true);

        public abstract TData GetData(SourcePriority priority);

        public abstract void SetData(TData data);

        public abstract void SetUserData(string userId, string version);

        public abstract UnityTask LoadAsync(SourcePriority priority, CancellationToken token);

        public abstract UnityTask SaveAsync(SaveDestination destination, CancellationToken token);

        public abstract Option<TData> TryCloneData(SourcePriority priority);

        public abstract bool TryCloneDataFromCloud();
    }
}

#endif
