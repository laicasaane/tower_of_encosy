#if !UNITASK && UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __MODULE_CORE_PUBSUB_NO_VALIDATION__
#else
#define __MODULE_CORE_PUBSUB_VALIDATION__
#endif

using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Module.Core.PubSub
{
    using CallerInfo = Module.Core.Logging.CallerInfo;
    using DevLogger = Module.Core.Logging.DevLogger;

    partial struct CachedPublisher<TMessage>
    {
#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public readonly void Publish(
              TMessage message
            , CancellationToken token = default
            , Module.Core.Logging.ILogger logger = null
            , CallerInfo callerInfo = default
        )
        {
#if __MODULE_CORE_PUBSUB_VALIDATION__
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
            , Module.Core.Logging.ILogger logger = null
            , CallerInfo callerInfo = default
        )
        {
            return PublishAsync(new TMessage(), token, logger, callerInfo);
        }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public readonly Awaitable PublishAsync(
              TMessage message
            , CancellationToken token = default
            , Module.Core.Logging.ILogger logger = null
            , CallerInfo callerInfo = default
        )
        {
#if __MODULE_CORE_PUBSUB_VALIDATION__
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
