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
        private readonly RootPath _rootPath;
        private readonly string _fileExtension;
        private readonly TransformFunc<TData, string> _serializeFunc;
        private readonly TransformFunc<string, TData> _deserializeFunc;
        private readonly MakeFilePathFunc _makeFilePathFunc;

        private string _userId;
        private string _fileName;
        private string _subFolderName;
        private string _filePath;

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

            if (deviceArgs.RootPath.IsValid == false)
            {
                throw CreateArgumentException_RootPathInvalid();
            }

            _rootPath = deviceArgs.RootPath;

#if ENFORCE_USER_DATA_ENCRYPTION
            var fileExtension = "enc";
#else
            var fileExtension = deviceArgs.FileExtension.NotEmptyOr(ignoreEncryption ? "txt" : "enc");
#endif

            _fileExtension = fileExtension;
            _serializeFunc = deviceArgs.SerializeFunc;
            _deserializeFunc = deviceArgs.DeserializeFunc;
            _makeFilePathFunc = deviceArgs.MakeFilePathFunc;
        }

        public bool IsInitialized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _filePath.IsNotEmpty();
        }

        public string FilePath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _filePath;
        }

        public string UserId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _userId;
            }

            set
            {
                _userId = value;

                if (IsInitialized == false)
                {
                    return;
                }

                MakeSubFolderName();
                MakeFilePath();
            }
        }

        public override void Initialize()
        {
            if (string.IsNullOrEmpty(_userId))
            {
                throw CreateInvalidOperationException_UserIdNotSet();
            }

            MakeFileName();
            MakeSubFolderName();
            MakeFilePath();

            var filePath = _filePath;
            var directoryPath = Path.GetDirectoryName(filePath);

            if (Directory.Exists(directoryPath) == false)
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        protected override async UnityTask OnSaveAsync([NotNull] TData data, CancellationToken token)
        {
            if (IsInitialized == false)
            {
                throw CreateInvalidOperationException_NotInitialized_SaveAsync();
            }

            try
            {
                var filePath = _filePath;

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
                if (IsInitialized == false)
                {
                    throw CreateInvalidOperationException_NotInitialized_TryLoadAsync();
                }

                var filePath = _filePath;

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

        private void MakeFileName()
        {
            using var _ = StringBuilderPool.Rent(out var sb);
            var value = StringVault.TryGetManagedString(Key).GetValueOrDefault(string.Empty);
            _fileName = PathAPI.ToFileName(value, sb);
        }

        private void MakeSubFolderName()
        {
            using var _ = StringBuilderPool.Rent(out var sb);
            _subFolderName = PathAPI.ToFileName(_userId, sb);
        }

        private void MakeFilePath()
        {
            _filePath = (_makeFilePathFunc?.Invoke(_rootPath, _subFolderName, _fileName, _fileExtension, typeof(TData)))
                .NotEmptyOr(Path.Combine(_rootPath.Root, _subFolderName, $"{_fileName}.{_fileExtension}"));
        }

        private static Exception CreateArgumentException_InstanceOfType()
            => new ArgumentException($"'args' must be an instance of '{typeof(Args).FullName}'.");

        private static Exception CreateInvalidOperationException_NotInitialized_SaveAsync()
            => new InvalidOperationException("The data source must be initialized before calling SaveAsync.");

        private static Exception CreateInvalidOperationException_NotInitialized_TryLoadAsync()
            => new InvalidOperationException("The data source must be initialized before calling TryLoadAsync.");

        private static Exception CreateInvalidOperationException_UserIdNotSet()
            => new InvalidOperationException("'UserId' must be set before calling this method.");

        private static Exception CreateArgumentException_RootPathInvalid()
            => new ArgumentException("'RootPath' must be a valid rooted directory path.", "args");

        public sealed record class Args(
              RootPath RootPath
            , [NotNull] TransformFunc<TData, string> SerializeFunc
            , [NotNull] TransformFunc<string, TData> DeserializeFunc
            , string FileExtension = null
            , MakeFilePathFunc MakeFilePathFunc = null
        ) : UserDataSourceArgs;
    }
}

#endif
