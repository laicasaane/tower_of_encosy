#if !UNITASK && UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __MODULE_CORE_PUBSUB_NO_VALIDATION__
#else
#define __MODULE_CORE_PUBSUB_VALIDATION__
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Module.Core.PubSub.Internals;
using UnityEngine;

namespace Module.Core.PubSub
{
    public partial class AnonSubscriber
    {
        partial struct Subscriber<TScope>
        {
#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe(
                  [NotNull] Func<CancellationToken, Awaitable> handler
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<AnonMessage>.None;
                }
#endif

                return _subscriber.Subscribe<AnonMessage>(handler, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe(
                  [NotNull] Func<Awaitable> handler
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<AnonMessage>.None;
                }
#endif

                return _subscriber.Subscribe<AnonMessage>(handler, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe(
                  [NotNull] Func<CancellationToken, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<AnonMessage>(handler, unsubscribeToken, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe(
                  [NotNull] Func<Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<AnonMessage>(handler, unsubscribeToken, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe(
                  [NotNull] Func<PublishingContext, CancellationToken, Awaitable> handler
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<AnonMessage>.None;
                }
#endif

                return _subscriber.Subscribe<AnonMessage>(handler, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe(
                  [NotNull] Func<PublishingContext, Awaitable> handler
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<AnonMessage>.None;
                }
#endif

                return _subscriber.Subscribe<AnonMessage>(handler, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe(
                  [NotNull] Func<PublishingContext, CancellationToken, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<AnonMessage>(handler, unsubscribeToken, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe(
                  [NotNull] Func<PublishingContext, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<AnonMessage>(handler, unsubscribeToken, order, logger);
            }
        }
    }
}

#endif
