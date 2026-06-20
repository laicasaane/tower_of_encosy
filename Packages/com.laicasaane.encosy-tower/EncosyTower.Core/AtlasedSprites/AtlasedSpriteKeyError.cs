using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.PolyEnumStructs;
using EncosyTower.Pooling;

namespace EncosyTower.AtlasedSprites
{
    [PolyEnumFactoryFor(typeof(Error))]
    public readonly partial struct AtlasedSpriteKeyError
    {
        private readonly string _prefix;
        private readonly Error _error;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AtlasedSpriteKeyError(in Error error) : this(error, default)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AtlasedSpriteKeyError(in Error error, string prefix)
        {
            _prefix = prefix;
            _error = error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AtlasedSpriteKeyError Prefix(string prefix)
            => new(_error, prefix);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => _error.ToMessage(_prefix).ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Log([NotNull] Logging.ILogger logger)
            => _error.Log(logger, _prefix);

        [PolyEnumStruct]
        readonly partial struct Error
        {
            partial interface IEnumCase
            {
                string ToMessage(string prefix);

                void Log(Logging.ILogger logger, string prefix);
            }

            public readonly partial record struct Undefined(AtlasedSpriteKey Key)
            {
                public string ToMessage(string prefix)
                {
                    const string MSG1 = "Unknown error occurred while loading the atlased sprite key '";
                    const string MSG2 = "'.";

                    return GetMessage(prefix, MSG1, Key, MSG2);
                }

                public void Log(Logging.ILogger logger, string prefix)
                {
                    logger.LogError(ToMessage(prefix));
                }
            }

            public readonly partial record struct InvalidKey(AtlasedSpriteKey Key)
            {
                public string ToMessage(string prefix)
                {
                    const string MSG1 = "The atlased sprite key '";
                    const string MSG2 = "' is invalid.";

                    return GetMessage(prefix, MSG1, Key, MSG2);
                }

                public void Log(Logging.ILogger logger, string prefix)
                {
                    logger.LogError(ToMessage(prefix));
                }
            }

            public readonly partial record struct InvalidSpriteKey(AssetKey Key)
            {
                public string ToMessage(string prefix)
                {
                    const string MSG1 = "The sprite key '";
                    const string MSG2 = "' is invalid.";

                    return GetMessage(prefix, MSG1, Key, MSG2);
                }

                public void Log(Logging.ILogger logger, string prefix)
                {
                    logger.LogError(ToMessage(prefix));
                }
            }

            public readonly partial record struct CancelledRequest(AtlasedSpriteKey Key)
            {
                public string ToMessage(string prefix)
                {
                    const string MSG1 = "The loading request for the atlased sprite key '";
                    const string MSG2 = "' was cancelled.";

                    return GetMessage(prefix, MSG1, Key, MSG2);
                }

                public void Log(Logging.ILogger logger, string prefix)
                {
                    logger.LogError(ToMessage(prefix));
                }
            }

            public readonly partial record struct InvalidObject(AtlasedSpriteKey Key)
            {
                public string ToMessage(string prefix)
                {
                    const string MSG1 = "The object loaded with the atlased sprite key '";
                    const string MSG2 = "' is invalid.";

                    return GetMessage(prefix, MSG1, Key, MSG2);
                }

                public void Log(Logging.ILogger logger, string prefix)
                {
                    logger.LogError(ToMessage(prefix));
                }
            }

            public readonly partial record struct InvalidObjectOfType(
                  AtlasedSpriteKey Key
                , System.Type ExpectedType
                , System.Type ActualType
            )
            {
                public string ToMessage(string prefix)
                {
                    const string MSG1 = "The object loaded with the atlased sprite key '";
                    const string MSG2 = "' is of type '";
                    const string MSG3 = "', but expected type is '";
                    const string MSG4 = "'.";

                    using var _ = StringBuilderPool.Rent(out var sb);
                    return InitString(sb, prefix).Append(MSG1).Append(Key.ToString()).Append(MSG2)
                        .Append(ActualType.FullName).Append(MSG3).Append(ExpectedType.FullName)
                        .Append(MSG4).ToString();
                }

                public void Log(Logging.ILogger logger, string prefix)
                {
                    logger.LogError(ToMessage(prefix));
                }
            }

            public readonly partial record struct Exception(AtlasedSpriteKey Key, System.Exception Ex)
            {
                public string ToMessage(string prefix)
                    => GetException(prefix).ToString();

                private System.Exception GetException(string prefix)
                {
                    const string MSG1 = "Exception occurred while loading the atlased sprite key '";
                    const string MSG2 = "'.";

                    return new System.InvalidOperationException(GetMessage(prefix, MSG1, Key, MSG2), Ex);
                }

                public void Log(Logging.ILogger logger, string prefix)
                {
                    logger.LogException(GetException(prefix));
                }
            }

            public readonly partial record struct SpriteException(AssetKey Key, System.Exception Ex)
            {
                public string ToMessage(string prefix)
                    => GetException(prefix).ToString();

                private System.Exception GetException(string prefix)
                {
                    const string MSG1 = "Exception occurred while loading the sprite key '";
                    const string MSG2 = "'.";

                    return new System.InvalidOperationException(GetMessage(prefix, MSG1, Key, MSG2), Ex);
                }

                public void Log(Logging.ILogger logger, string prefix)
                {
                    logger.LogException(GetException(prefix));
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static string GetMessage(string prefix, string m1, AtlasedSpriteKey key, string m2)
            {
                using var _ = StringBuilderPool.Rent(out var sb);
                return InitString(sb, prefix).Append(m1).Append(key.ToString()).Append(m2).ToString();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static string GetMessage(string prefix, string m1, AssetKey key, string m2)
            {
                using var _ = StringBuilderPool.Rent(out var sb);
                return InitString(sb, prefix).Append(m1).Append(key.ToString()).Append(m2).ToString();
            }

            private static StringBuilder InitString(StringBuilder sb, string prefix)
            {
                if (prefix.IsEmpty() == false)
                {
                    sb.Append('[');
                    sb.Append(prefix);
                    sb.Append(']');
                    sb.Append(' ');
                }

                return sb;
            }
        }
    }
}
