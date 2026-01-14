#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Conversion;
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

    public class UserDataStoreOnDevice<TData> : UserDataStoreLocation<TData>
        where TData : IUserData
    {
        private readonly string _directoryPath;
        private readonly string _filePath;
        private readonly TransformFunc<TData, string> _serializeFunc;
        private readonly TransformFunc<string, TData> _deserializeFunc;

        public UserDataStoreOnDevice(
              [NotNull] StringId<string> key
            , [NotNull] EncryptionBase encryption
            , ILogger logger
            , bool ignoreEncryption
            , [NotNull] UserDataStorageArgs args
        )
            : base(key, encryption, logger, ignoreEncryption, args)
        {
            if (args is not Args deviceArgs)
            {
                throw new ArgumentException(
                      $"'{nameof(args)}' must be an instance of '{typeof(Args)}'."
                    , nameof(args)
                );
            }

            _directoryPath = deviceArgs.DirectoryPath;

            var fileName = IdToString.GetManaged(Key);

#if FORCE_USER_DATA_ENCRYPTION
            var fileExtension = "user";
#else
            var fileExtension = deviceArgs.FileExtension.NotEmptyOr(ignoreEncryption ? "txt" : "user");
#endif

            _filePath = Path.Combine(_directoryPath, $"{fileName}.{fileExtension}");
            _serializeFunc = deviceArgs.SerializeFunc;
            _deserializeFunc = deviceArgs.DeserializeFunc;
        }

        public override void Initialize()
        {
            if (Directory.Exists(_directoryPath) == false)
            {
                Directory.CreateDirectory(_directoryPath);
            }
        }

        protected override async UnityTask OnSaveAsync([NotNull] TData data, CancellationToken token)
        {
            try
            {
                if (_serializeFunc(data, out var text))
                {
#if FORCE_USER_DATA_ENCRYPTION
                    var raw = Encryption.Encrypt(text);
#else
                    var raw = IgnoreEncryption ? text : Encryption.Encrypt(text);
#endif

                    await File.WriteAllTextAsync(_filePath, raw, System.Text.Encoding.UTF8).AsUnityTask();
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
                if (File.Exists(_filePath))
                {
                    var raw = await File.ReadAllTextAsync(_filePath, token).AsUnityTask();

#if FORCE_USER_DATA_ENCRYPTION
                    var text = Encryption.Decrypt(raw);
#else
                    var text = IgnoreEncryption ? raw : Encryption.Decrypt(raw);
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

            return Option<TData>.None;
        }

        public class Args : UserDataStorageArgs
        {
            public readonly string DirectoryPath;
            public readonly TransformFunc<TData, string> SerializeFunc;
            public readonly TransformFunc<string, TData> DeserializeFunc;
            public readonly string FileExtension;

            public Args(
                  [NotNull] string directoryPath
                , [NotNull] TransformFunc<TData, string> serializeFunc
                , [NotNull] TransformFunc<string, TData> deserializeFunc
                , string fileExtension = null
            )
            {
                DirectoryPath = directoryPath;
                SerializeFunc = serializeFunc;
                DeserializeFunc = deserializeFunc;
                FileExtension = fileExtension;
            }
        }

    }
}

#endif
