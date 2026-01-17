#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PubSub.Internals;

namespace EncosyTower.PubSub
{
    public static partial class MessageSubscriberExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MessageSubscriber.Subscriber<TScope> WithSubscriptions<TScope>(
              this in MessageSubscriber.Subscriber<TScope> subscriber
            , ICollection<ISubscription> subscriptions
        )
        {
            return new MessageSubscriber.Subscriber<TScope>(
                  subscriber._subscriber
                , subscriber.Scope
                , subscriptions ?? EmptySubscriptions.Default
            );
        }
    }

    partial class MessageSubscriber
    {
        public readonly partial struct Subscriber<TScope> : IIsCreated
        {
            internal readonly MessageSubscriber _subscriber;
            internal readonly ICollection<ISubscription> _subscriptions;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Subscriber([NotNull] MessageSubscriber subscriber, [NotNull] TScope scope)
            {
                _subscriber = subscriber;
                _subscriptions = EmptySubscriptions.Default;
                Scope = scope;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Subscriber(
                  [NotNull] MessageSubscriber subscriber
                , [NotNull] TScope scope
                , [NotNull] ICollection<ISubscription> subscriptions
            )
            {
                _subscriber = subscriber;
                _subscriptions = subscriptions;
                Scope = scope;
            }

            public bool IsCreated => _subscriber != null;

            public TScope Scope { get; }

            public ICollection<ISubscription> Subscriptions => _subscriptions ?? EmptySubscriptions.Default;

            /// <summary>
            /// Remove empty handler groups to optimize performance.
            /// </summary>
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Compress<TMessage>(ILogger logger = null)
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                if (_subscriber._brokers.TryGet<MessageBroker<TScope, TMessage>>(out var broker))
                {
                    broker.Compress(Scope, logger);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Action handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                TrySubscribe(new HandlerAction<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Action<TMessage> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                TrySubscribe(new HandlerActionMessage<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Action handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                if (TrySubscribe(new HandlerAction<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Action<TMessage> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                if (TrySubscribe(new HandlerActionMessage<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Action<PublishingContext> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                TrySubscribe(new ContextualHandlerAction<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Action<TMessage, PublishingContext> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                TrySubscribe(new ContextualHandlerActionMessage<TMessage>(handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Action<PublishingContext> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                if (TrySubscribe(new ContextualHandlerAction<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Action<TMessage, PublishingContext> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                if (TrySubscribe(new ContextualHandlerActionMessage<TMessage>(handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

            internal bool TrySubscribe<TMessage>(
                  IHandler<TMessage> handler
                , int order
                , out Subscription<TMessage> subscription
                , ILogger logger
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    subscription = Subscription<TMessage>.None;
                    return false;
                }
#endif

                var taskArrayPool = _subscriber._taskArrayPool;
                var brokers = _subscriber._brokers;

                lock (brokers)
                {
                    if (brokers.TryGet<MessageBroker<TScope, TMessage>>(out var broker) == false)
                    {
                        broker = new MessageBroker<TScope, TMessage>();

                        if (brokers.TryAdd(broker) == false)
                        {
                            broker?.Dispose();
                            subscription = Subscription<TMessage>.None;
                            return false;
                        }
                    }

                    subscription = broker.Subscribe(Scope, handler, order, taskArrayPool, logger);
                }

                Subscriptions.Add(subscription);
                return true;
            }

#if __ENCOSY_VALIDATION__
            private bool Validate(ILogger logger)
            {
                if (_subscriber != null)
                {
                    return true;
                }

                (logger ?? DevLogger.Default).LogError(
                    $"{GetType()} must be retrieved via `{nameof(MessageSubscriber)}.{nameof(MessageSubscriber.Scope)}` API"
                );

                return false;
            }
#endif

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void RetainUsings();
        }
    }
}

#endif
