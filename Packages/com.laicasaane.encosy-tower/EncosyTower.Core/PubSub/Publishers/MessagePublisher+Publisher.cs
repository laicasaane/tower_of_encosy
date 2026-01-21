#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PubSub.Internals;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub
{
    partial class MessagePublisher
    {
        public readonly partial struct Publisher<TScope> : IIsCreated
        {
            internal readonly MessagePublisher _publisher;

            public bool IsCreated => _publisher != null;

            public TScope Scope { get; }

            internal Publisher([NotNull] MessagePublisher publisher, [NotNull] TScope scope)
            {
                _publisher = publisher;
                Scope = scope;
            }

            public CachedPublisher<TScope, TMessage> Cache<TMessage>(
                  [NotNull] Func<TMessage> createFunc
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return default;
                }
#endif

                lock (_publisher._messageBrokers)
                {
                    var brokers = _publisher._messageBrokers;

                    if (brokers.TryGet<MessageBroker<TScope, TMessage>>(out var scopedBroker) == false)
                    {
                        scopedBroker = new MessageBroker<TScope, TMessage>();

                        if (brokers.TryAdd(scopedBroker) == false)
                        {
#if __ENCOSY_VALIDATION__
                            LogUnexpectedErrorWhenCache<TMessage>(logger);
#endif

                            scopedBroker.Dispose();
                            return default;
                        }
                    }

                    return new CachedPublisher<TScope, TMessage>(
                          scopedBroker.Cache(Scope, _publisher._taskArrayPool)
                        , _publisher._interceptorBrokers
                        , createFunc
                        , Scope
                    );
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Publish<TMessage>(PublishingContext context = default)
#if ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : new()
#else
                where TMessage : IMessage, new()
#endif
            {
                Publish(new TMessage(), context);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Publish<TMessage>(TMessage message, PublishingContext context = default)
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                PublishAsync(message, context).Forget();
            }

#if __ENCOSY_VALIDATION__
            private bool Validate(ILogger logger)
            {
                if (_publisher == null)
                {
                    (logger ?? DevLogger.Default).LogError(
                        $"{GetType()} must be retrieved via `{nameof(MessagePublisher)}.{nameof(MessagePublisher.Scope)}` API"
                    );

                    return false;
                }

                if (Scope != null)
                {
                    return true;
                }

                (logger ?? DevLogger.Default).LogException(new System.NullReferenceException(nameof(Scope)));
                return false;
            }

            private bool Validate<TMessage>(TMessage message, ILogger logger)
            {
                if (_publisher == null)
                {
                    (logger ?? DevLogger.Default).LogError(
                        $"{GetType()} must be retrieved via `{nameof(MessagePublisher)}.{nameof(MessagePublisher.Scope)}` API"
                    );

                    return false;
                }

                if (Scope == null)
                {
                    (logger ?? DevLogger.Default).LogException(new System.NullReferenceException(nameof(Scope)));
                    return false;
                }

                if (message != null)
                {
                    return true;
                }

                (logger ?? DevLogger.Default).LogException(new System.ArgumentNullException(nameof(message)));
                return false;
            }

            private static void LogUnexpectedErrorWhenCache<TMessage>(ILogger logger)
            {
                (logger ?? DevLogger.Default).LogError(
                    $"Something went wrong when registering a new instance of {typeof(MessageBroker<TScope, TMessage>)}!"
                );
            }
#endif
        }
    }
}

#endif
