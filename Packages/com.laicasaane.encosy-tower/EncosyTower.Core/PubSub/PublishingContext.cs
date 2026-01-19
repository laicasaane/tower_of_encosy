#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using EncosyTower.Logging;

namespace EncosyTower.PubSub
{
    public readonly struct PublishingContext
    {
        private readonly ILogger _logger;
        private readonly CallerInfo _callerInfo;
        private readonly PublishingStrategy _strategy;
        private readonly bool _warnNoSubscriber;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PublishingContext(
              PublishingStrategy strategy
            , bool warmNoSubscriber
            , ILogger logger
            , in CallerInfo callerInfo
        )
        {
            _logger = logger;
            _strategy = strategy;
            _warnNoSubscriber = warmNoSubscriber;
            _callerInfo = callerInfo;
        }

        public PublishingStrategy Strategy => _strategy;

        public bool WarnNoSubscriber => _warnNoSubscriber;

        public ILogger Logger => _logger ?? DevLogger.Default;

        public CallerInfo CallerInfo => _callerInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PublishingContext Default(
              bool warnNoSubscriber = true
            , ILogger logger = default
            , in CallerInfo callerInfo = default
        )
        {
            return DropIfNoSubscriber(
                  warnNoSubscriber
                , logger
                , callerInfo
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PublishingContext DropIfNoSubscriber(
              bool warnNoSubscriber = true
            , ILogger logger = default
            , in CallerInfo callerInfo = default
        )
        {
            return new(
                  PublishingStrategy.DropIfNoSubscriber
                , warnNoSubscriber
                , logger
                , callerInfo
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PublishingContext WaitForSubscriber(
              ILogger logger = default
            , in CallerInfo callerInfo = default
        )
        {
            return new(
                  PublishingStrategy.WaitForSubscriber
                , false
                , logger
                , callerInfo
            );
        }
    }
}

#endif
