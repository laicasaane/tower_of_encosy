#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using EncosyTower.Logging;

namespace EncosyTower.PubSub
{
    public readonly struct PublishingContext
    {
        private readonly ILogger _logger;
        private readonly CallerInfo _callerInfo;
        private readonly bool _ignoreEmptySubscriber;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PublishingContext(bool ignoreEmptySubscriber, ILogger logger, in CallerInfo callerInfo)
        {
            _logger = logger;
            _callerInfo = callerInfo;
            _ignoreEmptySubscriber = ignoreEmptySubscriber;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PublishingContext Get(
              bool ignoreEmptySubscriber = false
            , ILogger logger = default
            , in CallerInfo callerInfo = default
        )
        {
            return new(ignoreEmptySubscriber, logger, callerInfo);
        }

        public ILogger Logger => _logger ?? DevLogger.Default;

        public CallerInfo CallerInfo => _callerInfo;

        public bool IgnoreEmptySubscriber => _ignoreEmptySubscriber;
    }
}

#endif
