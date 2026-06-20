#if UNITY_ADDRESSABLES

using EncosyTower.AddressableKeys;
using EncosyTower.Pooling;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EncosyTower.AtlasedSprites
{
    partial struct AtlasedSpriteKeyError
    {
        public static AtlasedSpriteKeyError From(AddressableKeyError error, AtlasedSpriteKey key)
        {
            var prefix = error._prefix;

            switch (error._error.enumCase)
            {
                case AddressableKeyError.Error.EnumCase.InvalidKey:
                    return InvalidKey(key).Prefix(prefix);

                case AddressableKeyError.Error.EnumCase.InvalidHandle:
                    return InvalidHandle(key);

                case AddressableKeyError.Error.EnumCase.CancelledRequest:
                    return CancelledRequest(key).Prefix(prefix);

                case AddressableKeyError.Error.EnumCase.FailedStatus:
                {
                    var typedError = (AddressableKeyError.Error.FailedStatus)error._error;
                    return FailedStatus(key, typedError.Status).Prefix(prefix);
                }

                case AddressableKeyError.Error.EnumCase.InvalidObject:
                    return InvalidObject(key).Prefix(prefix);

                case AddressableKeyError.Error.EnumCase.InvalidObjectOfType:
                {
                    var typedError = (AddressableKeyError.Error.InvalidObjectOfType)error._error;
                    return InvalidObjectOfType(key, typedError.ExpectedType, typedError.ActualType).Prefix(prefix);
                }

                case AddressableKeyError.Error.EnumCase.Exception:
                {
                    var typedError = (AddressableKeyError.Error.Exception)error._error;
                    return Exception(key, typedError.Ex).Prefix(prefix);
                }

                default:
                    return Undefined(key).Prefix(prefix);
            }
        }

        partial struct Error
        {
            public readonly partial record struct InvalidHandle(AtlasedSpriteKey Key)
            {
                public string ToMessage(string prefix)
                {
                    const string MSG1 = "The AsyncOperationHandle for the atlased sprite key '";
                    const string MSG2 = "' is invalid.";

                    return GetMessage(prefix, MSG1, Key, MSG2);
                }

                public void Log(Logging.ILogger logger, string prefix)
                {
                    logger.LogError(ToMessage(prefix));
                }
            }

            public readonly partial record struct FailedStatus(AtlasedSpriteKey Key, AsyncOperationStatus Status)
            {
                public string ToMessage(string prefix)
                {
                    const string MSG1 = "The loading request for the atlased sprite key '";
                    const string MSG2 = "' failed with status '";
                    const string MSG3 = "'.";

                    using var _ = StringBuilderPool.Rent(out var sb);
                    return InitString(sb, prefix).Append(MSG1).Append(Key.ToString()).Append(MSG2)
                        .Append(Status.ToString()).Append(MSG3).ToString();
                }

                public void Log(Logging.ILogger logger, string prefix)
                {
                    logger.LogError(ToMessage(prefix));
                }
            }
        }
    }
}

#endif
