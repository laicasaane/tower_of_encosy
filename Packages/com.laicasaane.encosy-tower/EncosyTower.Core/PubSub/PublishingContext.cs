#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Logging;

namespace EncosyTower.PubSub
{
    public readonly struct PublishingContext
    {
        public PublishingStrategy Strategy { get; init; }

        public bool WarnNoSubscriber { get; init; }

        public ILogger Logger { get; init; }

        public CallerInfo CallerInfo { get; init; }

        public CancellationToken Token { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PublishingContext Default(
              bool warnNoSubscriber = true
            , ILogger logger = default
            , in CallerInfo callerInfo = default
            , CancellationToken token = default
        )
        {
            return DropIfNoSubscriber(
                  warnNoSubscriber
                , logger
                , callerInfo
                , token
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PublishingContext DropIfNoSubscriber(
              bool warnNoSubscriber = true
            , ILogger logger = default
            , in CallerInfo callerInfo = default
            , CancellationToken token = default
        )
        {
            return new PublishingContext() {
                Strategy = PublishingStrategy.DropIfNoSubscriber,
                WarnNoSubscriber = warnNoSubscriber,
                Logger = logger,
                CallerInfo = callerInfo,
                Token = token,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PublishingContext WaitForSubscriber(
              ILogger logger = default
            , in CallerInfo callerInfo = default
            , CancellationToken token = default
        )
        {
            return new PublishingContext() {
                Strategy = PublishingStrategy.WaitForSubscriber,
                WarnNoSubscriber = false,
                Logger = logger,
                CallerInfo = callerInfo,
                Token = token
            };
        }
    }
}

#endif
