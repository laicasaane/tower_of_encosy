using EncosyTower.ResourceKeys;

namespace EncosyTower.AtlasedSprites
{
    partial struct AtlasedSpriteKeyError
    {
        public static AtlasedSpriteKeyError From(ResourceKeyError error, AtlasedSpriteKey key)
        {
            var prefix = error._prefix;

            switch (error._error.enumCase)
            {
                case ResourceKeyError.Error.EnumCase.InvalidKey:
                    return InvalidKey(key).Prefix(prefix);

                case ResourceKeyError.Error.EnumCase.InvalidRequest:
                    return InvalidRequest(key).Prefix(prefix);

                case ResourceKeyError.Error.EnumCase.CancelledRequest:
                    return CancelledRequest(key).Prefix(prefix);

                case ResourceKeyError.Error.EnumCase.InvalidObject:
                    return InvalidObject(key).Prefix(prefix);

                case ResourceKeyError.Error.EnumCase.InvalidObjectOfType:
                {
                    var typedError = (ResourceKeyError.Error.InvalidObjectOfType)error._error;
                    return InvalidObjectOfType(key, typedError.ExpectedType, typedError.ActualType).Prefix(prefix);
                }

                case ResourceKeyError.Error.EnumCase.Exception:
                {
                    var typedError = (ResourceKeyError.Error.Exception)error._error;
                    return Exception(key, typedError.Ex).Prefix(prefix);
                }

                default:
                    return Undefined(key).Prefix(prefix);
            }
        }

        partial struct Error
        {
            public readonly partial record struct InvalidRequest(AtlasedSpriteKey Key)
            {
                public string ToMessage(string prefix)
                {
                    const string MSG1 = "The ResourceRequest for the atlased sprite key '";
                    const string MSG2 = "' is invalid.";

                    return GetMessage(prefix, MSG1, Key, MSG2);
                }

                public void Log(Logging.ILogger logger, string prefix)
                {
                    logger.LogError(ToMessage(prefix));
                }
            }
        }
    }
}
