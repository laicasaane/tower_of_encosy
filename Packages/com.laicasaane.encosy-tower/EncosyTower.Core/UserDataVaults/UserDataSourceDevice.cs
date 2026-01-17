#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Conversion;
using EncosyTower.Encryption;
using EncosyTower.IO;
using EncosyTower.Logging;
using EncosyTower.Pooling;
using EncosyTower.StringIds;
using EncosyTower.Tasks;

namespace EncosyTower.UserDataVaults
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public class UserDataSourceDevice<TData> : UserDataSourceBase<TData>
        where TData : IUserData
    {
        private readonly string _directoryPath;
        private readonly string _fileExtension;
        private readonly TransformFunc<TData, string> _serializeFunc;
        private readonly TransformFunc<string, TData> _deserializeFunc;

        public UserDataSourceDevice(
              StringId<string> key
            , [NotNull] StringVault stringVault
            , [NotNull] EncryptionBase encryption
            , ILogger logger
            , bool ignoreEncryption
            , [NotNull] UserDataSourceArgs args
        )
            : base(key, stringVault, encryption, logger, ignoreEncryption, args)
        {
            if (args is not Args deviceArgs)
            {
                throw CreateArgumentException_InstanceOfType();
            }

            _directoryPath = deviceArgs.DirectoryPath;

#if ENFORCE_USER_DATA_ENCRYPTION
            var fileExtension = "enc";
#else
            var fileExtension = deviceArgs.FileExtension.NotEmptyOr(ignoreEncryption ? "txt" : "enc");
#endif

            _fileExtension = fileExtension;
            _serializeFunc = deviceArgs.SerializeFunc;
            _deserializeFunc = deviceArgs.DeserializeFunc;
        }

        public string UserId { get; set; }

        private string UserIdFileName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                using var _ = StringBuilderPool.Rent(out var sb);
                return PathAPI.ToFileName(UserId, sb);
            }
        }

        private string KeyFileName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                using var _ = StringBuilderPool.Rent(out var sb);
                return PathAPI.ToFileName(StringVault.TryGetManagedString(Key).GetValueOrThrow(), sb);
            }
        }

        private string FilePath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Path.Combine(_directoryPath, UserIdFileName, $"{KeyFileName}.{_fileExtension}");
        }

        public override void Initialize()
        {
            if (string.IsNullOrEmpty(UserId))
            {
                throw CreateInvalidOperationException_UserIdNotSet();
            }

            var filePath = FilePath;
            var directoryPath = Path.GetDirectoryName(filePath);

            if (Directory.Exists(directoryPath) == false)
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        protected override async UnityTask OnSaveAsync([NotNull] TData data, CancellationToken token)
        {
            try
            {
                var filePath = FilePath;

                if (_serializeFunc(data, out var text))
                {
#if ENFORCE_USER_DATA_ENCRYPTION
                    var raw = Encryption.Encrypt(text);
#else
                    var raw = (IgnoreEncryption || Encryption.IsInitialized == false)
                        ? text
                        : Encryption.Encrypt(text);
#endif

                    await File.WriteAllTextAsync(filePath, raw, Encoding.UTF8, token).AsUnityTask();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public override async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask
#else
            UnityEngine.Awaitable
#endif
            <Option<TData>> TryLoadAsync(CancellationToken token)
        {
            try
            {
                var filePath = FilePath;

                if (File.Exists(filePath))
                {
                    var raw = await File.ReadAllTextAsync(filePath, token).AsUnityTask();

                    if (token.IsCancellationRequested)
                    {
                        return Option.None;
                    }

#if ENFORCE_USER_DATA_ENCRYPTION
                    var text = Encryption.Decrypt(raw);
#else
                    var text = (IgnoreEncryption || Encryption.IsInitialized == false)
                        ? raw
                        : Encryption.Decrypt(raw);
#endif

                    if (_deserializeFunc(text, out TData data) && data != null)
                    {
                        return Option.Some(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return Option.None;
        }

        public override Option<TData> TryCloneData(TData source)
        {
            if (_serializeFunc(source, out var text) == false)
            {
                return Option.None;
            }

            if (_deserializeFunc(text, out var dest))
            {
                return Option.Some(dest);
            }

            return Option.None;
        }

        private static Exception CreateArgumentException_InstanceOfType()
            => new ArgumentException($"'args' must be an instance of '{typeof(Args).FullName}'.");

        private static Exception CreateInvalidOperationException_UserIdNotSet()
            => new InvalidOperationException("'UserId' must be set before calling this method.");

        public sealed record class Args(
              [NotNull] string DirectoryPath
            , [NotNull] TransformFunc<TData, string> SerializeFunc
            , [NotNull] TransformFunc<string, TData> DeserializeFunc
            , string FileExtension = null
        ) : UserDataSourceArgs;
    }
}

#endif
