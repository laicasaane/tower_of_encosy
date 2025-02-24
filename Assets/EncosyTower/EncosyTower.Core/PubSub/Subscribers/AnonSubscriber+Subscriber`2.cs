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
using EncosyTower.Logging;
using EncosyTower.PubSub.Internals;

namespace EncosyTower.PubSub
{
    public static partial class AnonSubscriberExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnonSubscriber.Subscriber<TScope, TState> WithState<TScope, TState>(
              this AnonSubscriber.Subscriber<TScope> subscriber
            , [NotNull] TState state
        )
            where TState : class
        {
            return new AnonSubscriber.Subscriber<TScope, TState>(subscriber, state);
        }
    }

    public partial class AnonSubscriber
    {
        /// <summary>
        /// Anonymous Subscriber allows registering handlers that take no message argument
        /// </summary>
        public readonly partial struct Subscriber<TScope, TState>
            where TState : class
        {
            internal readonly MessageSubscriber.Subscriber<TScope> _subscriber;

            public bool IsValid => _subscriber.IsValid;

            public TScope Scope => _subscriber.Scope;

            public TState State { get; }

            internal Subscriber(Subscriber<TScope> subscriber, [NotNull] TState state)
            {
                _subscriber = subscriber._subscriber;
                State = state;
            }

            /// <summary>
            /// Remove empty handler groups to optimize performance.
            /// </summary>
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Compress(ILogger logger = null)
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return;
#endif

                _subscriber.Compress<AnonMessage>(logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe(
                  [NotNull] Action<TState> handler
                , int order = 0
                , ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return Subscription<AnonMessage>.None;
#endif

                _subscriber.TrySubscribe(new StatefulHandlerAction<TState, AnonMessage>(State, handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe(
                  [NotNull] Action<TState> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return;
#endif

                if (_subscriber.TrySubscribe(new StatefulHandlerAction<TState, AnonMessage>(State, handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe(
                  [NotNull] Action<TState, PublishingContext> handler
                , int order = 0
                , ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return Subscription<AnonMessage>.None;
#endif

                _subscriber.TrySubscribe(new StatefulContextualHandlerAction<TState, AnonMessage>(State, handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe(
                  [NotNull] Action<TState, PublishingContext> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return;
#endif

                if (_subscriber.TrySubscribe(new StatefulContextualHandlerAction<TState, AnonMessage>(State, handler), order, out var subscription, logger))
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
                    $"{GetType().Name} must be retrieved via `{nameof(AnonSubscriber)}.{nameof(AnonSubscriber.Scope)}` API"
                );

                return false;
            }
#endif
        }
    }
}

#endif
