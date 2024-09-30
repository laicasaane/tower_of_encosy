#if !UNITASK && UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __MODULE_CORE_PUBSUB_NO_VALIDATION__
#else
#define __MODULE_CORE_PUBSUB_VALIDATION__
#endif

using System.Threading;
using Module.Core.PubSub.Internals;
using UnityEngine;

namespace Module.Core.PubSub
{
    using CallerInfo = Module.Core.Logging.CallerInfo;
    using DevLogger = Module.Core.Logging.DevLogger;

    partial class MessagePublisher
    {
        partial struct Publisher<TScope>
        {
#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Publish<TMessage>(
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
                    return;
                }
#endif

                if (_publisher._brokers.TryGet<MessageBroker<TScope, TMessage>>(out var broker))
                {
                    broker.PublishAsync(Scope, new TMessage(), new(callerInfo), token, logger ?? DevLogger.Default).Forget();
                }
#if __MODULE_CORE_PUBSUB_VALIDATION__
                else
                {
                    LogWarning<TMessage>(Scope, logger);
                }
#endif
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Publish<TMessage>(
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
                if (Validate(message, logger) == false)
                {
                    return;
                }
#endif

                if (_publisher._brokers.TryGet<MessageBroker<TScope, TMessage>>(out var broker))
                {
                    broker.PublishAsync(Scope, message, new(callerInfo), token, logger ?? DevLogger.Default).Forget();
                }
#if __MODULE_CORE_PUBSUB_VALIDATION__
                else
                {
                    LogWarning<TMessage>(Scope, logger);
                }
#endif
            }

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

                if (_publisher._brokers.TryGet<MessageBroker<TScope, TMessage>>(out var broker))
                {
                    return broker.PublishAsync(Scope, new TMessage(), new(callerInfo), token, logger ?? DevLogger.Default);
                }

#if __MODULE_CORE_PUBSUB_VALIDATION__
                else
                {
                    LogWarning<TMessage>(Scope, logger);
                }
#endif

                return Awaitables.GetCompleted();
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
                if (Validate(message, logger) == false)
                {
                    return Awaitables.GetCompleted();
                }
#endif

                if (_publisher._brokers.TryGet<MessageBroker<TScope, TMessage>>(out var broker))
                {
                    return broker.PublishAsync(Scope, message, new(callerInfo), token, logger ?? DevLogger.Default);
                }

#if __MODULE_CORE_PUBSUB_VALIDATION__
                else
                {
                    LogWarning<TMessage>(Scope, logger);
                }
#endif

                return Awaitables.GetCompleted();
            }
        }
    }
}

#endif
