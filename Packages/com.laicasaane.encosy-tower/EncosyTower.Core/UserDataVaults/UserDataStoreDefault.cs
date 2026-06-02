#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Encryption;
using EncosyTower.StringIds;
using UnityEngine;

namespace EncosyTower.UserDataVaults
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public sealed class UserDataStoreDefault<TData> : UserDataStoreBase<TData>
        where TData : IUserData
    {
        private readonly UserDataSourceDevice<TData> _source;
        private readonly Func<TData> _createDataFunc;
        private readonly bool _isValueType;

        private string _userId;
        private TData _data;

        public UserDataStoreDefault(
              StringId<string> key
            , [NotNull] StringVault stringVault
            , [NotNull] EncryptionBase encryption
            , EncosyTower.Logging.ILogger logger
            , bool ignoreEncryption
            , [NotNull] UserDataStoreArgs args
        )
            : base(key, stringVault, encryption, logger, ignoreEncryption, args)
        {
            if (args is not Args storeArgs)
            {
                throw CreateArgumentException_InstanceOfType();
            }

            _userId = string.Empty;
            _createDataFunc = storeArgs.CreateDataFunc;
            _isValueType = typeof(TData).IsValueType;
            _source = new UserDataSourceDevice<TData>(
                  key
                , stringVault
                , encryption
                , logger
                , ignoreEncryption
                , storeArgs.SourceArgs
            );
        }

        public bool IsInitialized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source.IsInitialized;
        }

        public string FilePath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source.FilePath;
        }

        public override string UserId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _userId;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _source.UserId = _userId = value ?? string.Empty;
        }

        public TData Data
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data;
        }

        public bool IsDataValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Data is not null;
        }

        public bool IsDataDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source.IsDirty;
        }

        public override void Initialize()
        {
            _source.Initialize();
        }

        public override void Deinitialize()
        {
        }

        public override void CreateData()
        {
            var data = _createDataFunc();
            data.Id = _userId ?? string.Empty;
            _data = data;
        }

        public override TData GetData(SourcePriority priority = default)
        {
            return _data;
        }

        /// <summary>
        /// Gets <typeparamref name="TData"/> by reference when it is a value type.
        /// </summary>
        /// <remarks>
        /// Should be used as an optimization when copy-by-value is not desirable.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Throws when <typeparamref name="TData"/> is not a value type.
        /// </exception>
        public ref TData GetDataByRef()
        {
            ThrowInvalidOperationIfNotValueType(_isValueType);
            return ref _data;
        }

        public override void SetData(TData data)
        {
            if (data is not null)
            {
                _data = data;
                _source.IsDirty = true;
            }
        }

        public override void SetUserData(string userId, string version)
        {
            ref var data = ref _data;
            data.Id = _userId = userId ?? string.Empty;
            data.Version = version ?? string.Empty;
        }

        public override void MarkDirty(bool isDirty = true)
        {
            _source.IsDirty = isDirty;
        }

        public override async UnityTask LoadAsync(SourcePriority priority = default, CancellationToken token = default)
        {
            var dataOpt = await _source.TryLoadAsync(token);

            if (dataOpt.TryGetValue(out var data))
            {
                _data = data;
            }
        }

        public override async UnityTask SaveAsync(SaveDestination destination = default, CancellationToken token = default)
        {
            if (IsDataValid && destination.Contains(SaveDestination.Device))
            {
                await _source.SaveAsync(Data, token);
            }
        }

        public override Option<TData> TryCloneData(SourcePriority priority = default)
        {
            return IsDataValid && _source.TryCloneData(GetData(priority)).TryGetValue(out var clone)
                ? Option.Some(clone)
                : Option.None;
        }

        public override bool TryCloneDataFromCloud()
        {
            return false;
        }

        private static Exception CreateArgumentException_InstanceOfType()
            => new ArgumentException($"'args' must be an instance of '{typeof(Args).FullName}'.");

        [HideInCallstack, StackTraceHidden]
        private static void ThrowInvalidOperationIfNotValueType(
              [DoesNotReturnIf(false)] bool check
            , [CallerMemberName] string memberName = ""
        )
        {
            if (check == false)
            {
                throw CreateException(memberName);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(string memberName)
                => new($"Cannot use 'UserDataStoreDefault<{typeof(TData)}>.{memberName}' " +
                    $"because {typeof(TData)} is not a value type."
                );
        }

        public sealed record class Args(
              [NotNull] Func<TData> CreateDataFunc
            , [NotNull] UserDataSourceArgs SourceArgs
        ) : UserDataStoreArgs;
    }
}

#endif
