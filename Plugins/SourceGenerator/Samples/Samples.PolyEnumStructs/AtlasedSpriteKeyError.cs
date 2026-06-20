using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.AtlasedSprites;
using EncosyTower.PolyEnumStructs;

namespace Samples.PolyEnumStructs.Errors3
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
        public void Log([NotNull] EncosyTower.Logging.ILogger logger)
            => _error.Log(logger, _prefix);

        [PolyEnumStruct]
        readonly partial struct Error
        {
            partial interface IEnumCase
            {
                string ToMessage(string prefix);

                void Log(EncosyTower.Logging.ILogger logger, string prefix);
            }

            public partial record struct Undefined(AtlasedSpriteKey Key)
            {
                public string ToMessage(string prefix)
                {
                    return default;
                }

                public void Log(EncosyTower.Logging.ILogger logger, string prefix)
                {
                    logger.LogError(ToMessage(prefix));
                }
            }

            public partial record struct InvalidKey(AtlasedSpriteKey Key)
            {
                public string ToMessage(string prefix)
                {
                    return default;
                }

                public void Log(EncosyTower.Logging.ILogger logger, string prefix)
                {
                    logger.LogError(ToMessage(prefix));
                }
            }
        }
    }
}
