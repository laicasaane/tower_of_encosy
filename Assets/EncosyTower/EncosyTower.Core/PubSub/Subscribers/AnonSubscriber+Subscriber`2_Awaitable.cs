#if !UNITASK && UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.PubSub.Internals;
using UnityEngine;

namespace EncosyTower.PubSub
{
    public partial class AnonSubscriber
    {
        partial struct Subscriber<TScope, TState>
        {
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe(
                  [NotNull] Func<TState, Awaitable> handler
                , int order = 0
                , EncosyTower.Logging.ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return Subscription<AnonMessage>.None;
#endif

                _subscriber.TrySubscribe(new StatefulHandlerFunc<TState, AnonMessage>(State, handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe(
                  [NotNull] Func<TState, CancellationToken, Awaitable> handler
                , int order = 0
                , EncosyTower.Logging.ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return Subscription<AnonMessage>.None;
#endif

                _subscriber.TrySubscribe(new StatefulHandlerFuncToken<TState, AnonMessage>(State, handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe(
                  [NotNull] Func<TState, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , EncosyTower.Logging.ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return;
#endif

                if (_subscriber.TrySubscribe(new StatefulHandlerFunc<TState, AnonMessage>(State, handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe(
                  [NotNull] Func<TState, CancellationToken, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , EncosyTower.Logging.ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return;
#endif

                if (_subscriber.TrySubscribe(new StatefulHandlerFuncToken<TState, AnonMessage>(State, handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe(
                  [NotNull] Func<TState, PublishingContext, Awaitable> handler
                , int order = 0
                , EncosyTower.Logging.ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return Subscription<AnonMessage>.None;
#endif

                _subscriber.TrySubscribe(new StatefulContextualHandlerFunc<TState, AnonMessage>(State, handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe(
                  [NotNull] Func<TState, PublishingContext, CancellationToken, Awaitable> handler
                , int order = 0
                , EncosyTower.Logging.ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return Subscription<AnonMessage>.None;
#endif

                _subscriber.TrySubscribe(new StatefulContextualHandlerFuncToken<TState, AnonMessage>(State, handler), order, out var subscription, logger);
                return subscription;
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe(
                  [NotNull] Func<TState, PublishingContext, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , EncosyTower.Logging.ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return;
#endif

                if (_subscriber.TrySubscribe(new StatefulContextualHandlerFunc<TState, AnonMessage>(State, handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe(
                  [NotNull] Func<TState, PublishingContext, CancellationToken, Awaitable> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , EncosyTower.Logging.ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false) return;
#endif

                if (_subscriber.TrySubscribe(new StatefulContextualHandlerFuncToken<TState, AnonMessage>(State, handler), order, out var subscription, logger))
                {
                    subscription.RegisterTo(unsubscribeToken);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void RetainUsings_Awaitable();
        }
    }
}

#endif
