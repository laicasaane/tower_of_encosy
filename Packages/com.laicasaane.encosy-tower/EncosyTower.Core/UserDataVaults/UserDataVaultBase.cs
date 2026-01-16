#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Initialization;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.UserDataVaults
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
    using UnityTaskBool = Cysharp.Threading.Tasks.UniTask<bool>;
#else
    using UnityTask = UnityEngine.Awaitable;
    using UnityTaskBool = UnityEngine.Awaitable<bool>;
#endif

    using ILogger = Logging.ILogger;

    public abstract class UserDataVaultBase : IDeinitializable, IDisposable
    {
        private bool _markDirtyBeforeSaving;

        protected abstract IUserDataDirectory DataDirectory { get; }

        public async UnityTaskBool TryLoadAsync(
              [NotNull] ILogger logger
            , string userId
            , SourcePriority priority
            , SaveDestination destination
            , CancellationToken token = default
        )
        {
            _markDirtyBeforeSaving = true;

            DataDirectory.UserId = userId;

            if (string.IsNullOrEmpty(userId))
            {
                LogWarningInvalidUserId(logger);
                return false;
            }

            DataDirectory.Initialize();

            await DataDirectory.LoadEntireDirectoryAsync(priority, token);

            if (token.IsCancellationRequested)
            {
                return false;
            }

            DataDirectory.CreateDataIfNotExist();

            var result = await OnTryLoadAsync(logger, userId, priority, destination, token);

            if (result == false || token.IsCancellationRequested)
            {
                return false;
            }

            await SaveAsync(destination, token: token);

            return true;
        }

        public void Deinitialize()
        {
            DataDirectory.Deinitialize();
            OnDeinitialize();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async UnityTask SaveAsync(SaveDestination destination, CancellationToken token = default)
        {
            if (_markDirtyBeforeSaving)
            {
                _markDirtyBeforeSaving = false;
                DataDirectory.MarkDirty(true);
            }

            await DataDirectory.SaveEntireDirectoryAsync(destination, token: token);
        }

        protected virtual UnityTaskBool OnTryLoadAsync(
              [NotNull] ILogger logger
            , string userId
            , SourcePriority priority
            , SaveDestination destination
            , CancellationToken token
        )
        {
            return UnityTasks.GetCompleted(true);
        }

        protected virtual void OnDeinitialize() { }

        protected virtual void Dispose(bool disposing) { }

        [HideInCallstack, StackTraceHidden]
        private static void LogWarningInvalidUserId(ILogger logger)
        {
            logger.LogWarning("User data cannot be loaded because 'userId' is invalid.");

        }
    }
}

#endif
