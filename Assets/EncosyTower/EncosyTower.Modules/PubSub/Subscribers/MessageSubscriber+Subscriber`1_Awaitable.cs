#if !UNITASK && UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __ENCOSY_PUBSUB_NO_VALIDATION__
#else
#define __ENCOSY_PUBSUB_VALIDATION__
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Modules.PubSub.Internals;
using UnityEngine;

namespace EncosyTower.Modules.PubSub
{
    partial class MessageSubscriber
    {
        partial struct Subscriber<TScope>
        {
#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<Awaitable> handler
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);
                TrySubscribe(new HandlerFunc<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<TMessage, Awaitable> handler
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);
                TrySubscribe(new HandlerFuncMessage<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<CancellationToken, Awaitable> handler
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);
                TrySubscribe(new HandlerFuncToken<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<TMessage, CancellationToken, Awaitable> handler
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);
                TrySubscribe(new HandlerFuncMessageToken<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);

                if (TrySubscribe(new HandlerFunc<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<TMessage, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);

                if (TrySubscribe(new HandlerFuncMessage<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<CancellationToken, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);

                if (TrySubscribe(new HandlerFuncToken<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<TMessage, CancellationToken, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);

                if (TrySubscribe(new HandlerFuncMessageToken<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<PublishingContext, Awaitable> handler
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);
                TrySubscribe(new ContextualHandlerFunc<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<TMessage, PublishingContext, Awaitable> handler
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);
                TrySubscribe(new ContextualHandlerFuncMessage<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<PublishingContext, CancellationToken, Awaitable> handler
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);
                TrySubscribe(new ContextualHandlerFuncToken<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Func<TMessage, PublishingContext, CancellationToken, Awaitable> handler
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);
                TrySubscribe(new ContextualHandlerFuncMessageToken<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<PublishingContext, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);

                if (TrySubscribe(new ContextualHandlerFunc<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<TMessage, PublishingContext, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);

                if (TrySubscribe(new ContextualHandlerFuncMessage<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<PublishingContext, CancellationToken, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);

                if (TrySubscribe(new ContextualHandlerFuncToken<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Func<TMessage, PublishingContext, CancellationToken, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , EncosyTower.Modules.Logging.ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowIfHandlerIsNull(handler);

                if (TrySubscribe(new ContextualHandlerFuncMessageToken<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }
        }
    }
}

#endif
