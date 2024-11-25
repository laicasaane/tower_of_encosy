#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Modules.Logging;
using EncosyTower.Modules.PubSub.Internals;

namespace EncosyTower.Modules.PubSub
{
    public static partial class MessageSubscriberExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MessageSubscriber.Subscriber<TScope, TState> WithState<TScope, TState>(
              this MessageSubscriber.Subscriber<TScope> subscriber
            , [NotNull] TState state
        )
            where TState : class
        {
            return new MessageSubscriber.Subscriber<TScope, TState>(subscriber, state);
        }
    }

    partial class MessageSubscriber
    {
        public readonly partial struct Subscriber<TScope, TState>
            where TState : class
        {
            internal readonly Subscriber<TScope> _subscriber;

            public bool IsValid => _subscriber.IsValid;

            public TScope Scope => _subscriber.Scope;

            public TState State { get; }

            internal Subscriber(Subscriber<TScope> subscriber, [NotNull] TState state)
            {
                _subscriber = subscriber;
                State = state;
            }

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
                if (Validate(logger) == false) return;
#endif

                _subscriber.Compress<TMessage>(logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Action<TState> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return Subscription<TMessage>.None;
#endif

                _subscriber.TrySubscribe(new StatefulHandlerAction<TState, TMessage>(State, handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Action<TState, TMessage> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return Subscription<TMessage>.None;
#endif

                _subscriber.TrySubscribe(new StatefulHandlerActionMessage<TState, TMessage>(State, handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Action<TState> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return;
#endif

                if (_subscriber.TrySubscribe(new StatefulHandlerAction<TState, TMessage>(State, handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Action<TState, TMessage> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return;
#endif

                if (_subscriber.TrySubscribe(new StatefulHandlerActionMessage<TState, TMessage>(State, handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Action<TState, PublishingContext> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return Subscription<TMessage>.None;
#endif

                _subscriber.TrySubscribe(new StatefulContextualHandlerAction<TState, TMessage>(State, handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe<TMessage>(
                  [NotNull] Action<TState, TMessage, PublishingContext> handler
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return Subscription<TMessage>.None;
#endif

                _subscriber.TrySubscribe(new StatefulContextualHandlerActionMessage<TState, TMessage>(State, handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Action<TState, PublishingContext> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return;
#endif

                if (_subscriber.TrySubscribe(new StatefulContextualHandlerAction<TState, TMessage>(State, handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe<TMessage>(
                  [NotNull] Action<TState, TMessage, PublishingContext> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return;
#endif

                if (_subscriber.TrySubscribe(new StatefulContextualHandlerActionMessage<TState, TMessage>(State, handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_VALIDATION__
            private bool Validate(ILogger logger)
            {
                if (IsValid)
                {
                    return true;
                }

                (logger ?? DevLogger.Default).LogError(
                    $"{GetType().Name} must be retrieved via `{nameof(MessageSubscriber)}.{nameof(MessageSubscriber.Scope)}` API"
                );

                return false;
            }
#endif
        }
    }
}

#endif
