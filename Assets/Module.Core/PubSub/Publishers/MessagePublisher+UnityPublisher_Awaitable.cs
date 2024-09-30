#if !UNITASK && UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __MODULE_CORE_PUBSUB_NO_VALIDATION__
#else
#define __MODULE_CORE_PUBSUB_VALIDATION__
#endif

using System.Threading;
using UnityEngine;

namespace Module.Core.PubSub
{
    partial class MessagePublisher
    {
        partial struct UnityPublisher<TScope>
        {
#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public Awaitable PublishAsync<TMessage>(
                  CancellationToken token = default
                , Module.Core.Logging.ILogger logger = null
                , CallerInfo callerInfo = default
            )
#if MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : new()
#else
                where TMessage : IMessage, new()
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Awaitables.GetCompleted();
                }
#endif

                return _publisher.PublishAsync<TMessage>(token, logger, callerInfo);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public Awaitable PublishAsync<TMessage>(
                  TMessage message
                , CancellationToken token = default
                , Module.Core.Logging.ILogger logger = null
                , CallerInfo callerInfo = default
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Awaitables.GetCompleted();
                }
#endif

                return _publisher.PublishAsync(message, token, logger, callerInfo);
            }
        }
    }
}

#endif
