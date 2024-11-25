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
    public partial class AnonSubscriber
    {
        /// <summary>
        /// Anonymous Subscriber allows registering handlers that take no message argument
        /// </summary>
        public readonly partial struct UnitySubscriber<TScope>
            where TScope : UnityEngine.Object
        {
            internal readonly MessageSubscriber.Subscriber<UnityInstanceId<TScope>> _subscriber;

            public UnityInstanceId<TScope> Scope => _subscriber.Scope;

            public bool IsValid => _subscriber.IsValid;

            internal UnitySubscriber(MessageSubscriber.UnitySubscriber<TScope> subscriber)
            {
                _subscriber = subscriber._subscriber;
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
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Compress<AnonMessage>(logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe(
                  [NotNull] Action handler
                , int order = 0
                , ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<AnonMessage>.None;
                }
#endif

                return _subscriber.Subscribe<AnonMessage>(handler, order, logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe(
                  [NotNull] Action handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<AnonMessage>(handler, unsubscribeToken, order, logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ISubscription Subscribe(
                  [NotNull] Action<PublishingContext> handler
                , int order = 0
                , ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Subscription<AnonMessage>.None;
                }
#endif

                return _subscriber.Subscribe<AnonMessage>(handler, order, logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Subscribe(
                  [NotNull] Action<PublishingContext> handler
                , CancellationToken unsubscribeToken
                , int order = 0
                , ILogger logger = null
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _subscriber.Subscribe<AnonMessage>(handler, unsubscribeToken, order, logger);
            }

#if __ENCOSY_VALIDATION__
            private bool Validate(ILogger logger)
            {
                if (IsValid)
                {
                    return true;
                }

                (logger ?? DevLogger.Default).LogError(
                    $"{GetType().Name} must be retrieved via `{nameof(AnonSubscriber)}.{nameof(UnityScope)}` API"
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
