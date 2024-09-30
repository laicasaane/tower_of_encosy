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
    partial class MessageSubscriber
    {
        partial struct UnitySubscriber<TScope>
        {
#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<Awaitable> handler
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<TMessage>.None;
                }
#endif

                return _subscriber.Subscribe<TMessage>(handler, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<TMessage, Awaitable> handler
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<TMessage>.None;
                }
#endif

                return _subscriber.Subscribe<TMessage>(handler, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<CancellationToken, Awaitable> handler
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<TMessage>.None;
                }
#endif

                return _subscriber.Subscribe<TMessage>(handler, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<TMessage, CancellationToken, Awaitable> handler
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<TMessage>.None;
                }
#endif

                return _subscriber.Subscribe<TMessage>(handler, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<TMessage>(handler, unsubscribeToken, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<TMessage, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<TMessage>(handler, unsubscribeToken, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<CancellationToken, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<TMessage>(handler, unsubscribeToken, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<TMessage, CancellationToken, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<TMessage>(handler, unsubscribeToken, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<PublishingContext, Awaitable> handler
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<TMessage>.None;
                }
#endif

                return _subscriber.Subscribe<TMessage>(handler, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<TMessage, PublishingContext, Awaitable> handler
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<TMessage>.None;
                }
#endif

                return _subscriber.Subscribe<TMessage>(handler, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<PublishingContext, CancellationToken, Awaitable> handler
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<TMessage>.None;
                }
#endif

                return _subscriber.Subscribe<TMessage>(handler, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<TMessage, PublishingContext, CancellationToken, Awaitable> handler
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<TMessage>.None;
                }
#endif

                return _subscriber.Subscribe<TMessage>(handler, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<PublishingContext, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<TMessage>(handler, unsubscribeToken, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<TMessage, PublishingContext, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<TMessage>(handler, unsubscribeToken, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<PublishingContext, CancellationToken, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<TMessage>(handler, unsubscribeToken, order, logger);
            }

#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<TMessage, PublishingContext, CancellationToken, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , Module.Core.Logging.ILogger logger = null
            )
#if !MODULE_CORE_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<TMessage>(handler, unsubscribeToken, order, logger);
            }
        }
    }
}

#endif
