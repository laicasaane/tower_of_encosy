#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using EncosyTower.Logging;

namespace EncosyTower.PubSub
{
    public readonly struct PublishingContext
    {
        private readonly ILogger _logger;
        private readonly CallerInfo _callerInfo;
        private readonly bool _warnNoSubscriber;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PublishingContext(
              bool warmNoSubscriber
            , ILogger logger
            , in CallerInfo callerInfo
        )
        {
            _logger = logger;
            _warnNoSubscriber = warmNoSubscriber;
            _callerInfo = callerInfo;
        }

        public bool WarnNoSubscriber => _warnNoSubscriber;

        public ILogger Logger => _logger ?? DevLogger.Default;

        public CallerInfo CallerInfo => _callerInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PublishingContext Get(
              bool warnNoSubscriber = true
            , ILogger logger = default
            , in CallerInfo callerInfo = default
        )
        {
            return new(
                  warnNoSubscriber
                , logger
                , callerInfo
            );
        }
    }
}

#endif
