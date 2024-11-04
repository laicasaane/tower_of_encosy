#if !UNITASK && UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __ENCOSY_PUBSUB_NO_VALIDATION__
#else
#define __ENCOSY_PUBSUB_VALIDATION__
#endif

using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Modules.PubSub
{
    using CallerInfo = EncosyTower.Modules.Logging.CallerInfo;
    using DevLogger = EncosyTower.Modules.Logging.DevLogger;

    partial struct CachedPublisher<TMessage>
    {
#if __ENCOSY_PUBSUB_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public readonly void Publish(
              TMessage message
            , CancellationToken token = default
            , EncosyTower.Modules.Logging.ILogger logger = null
            , CallerInfo callerInfo = default
        )
        {
#if __ENCOSY_PUBSUB_VALIDATION__
            if (Validate(message, logger) == false)
            {
                return;
            }
#endif

            _broker.PublishAsync(message, new(callerInfo), token, logger ?? DevLogger.Default).Forget();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Awaitable PublishAsync(
              CancellationToken token = default
            , EncosyTower.Modules.Logging.ILogger logger = null
            , CallerInfo callerInfo = default
        )
        {
            return PublishAsync(new TMessage(), token, logger, callerInfo);
        }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public readonly Awaitable PublishAsync(
              TMessage message
            , CancellationToken token = default
            , EncosyTower.Modules.Logging.ILogger logger = null
            , CallerInfo callerInfo = default
        )
        {
#if __ENCOSY_PUBSUB_VALIDATION__
            if (Validate(message, logger) == false)
            {
                return Awaitables.GetCompleted();
            }
#endif

            return _broker.PublishAsync(message, new(callerInfo), token, logger ?? DevLogger.Default);
        }
    }
}

#endif
