#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System.Runtime.CompilerServices;
using EncosyTower.PubSub.Internals;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    partial class MessagePublisher
    {
        partial struct Publisher<TScope>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityTask PublishAsync<TMessage>(PublishingContext context = default)
#if ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : new()
#else
                where TMessage : IMessage, new()
#endif
            {
                return PublishAsync(new TMessage(), context);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public UnityTask PublishAsync<TMessage>(TMessage message, PublishingContext context = default)
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(message, context.Logger) == false)
                {
                    return UnityTasks.GetCompleted();
                }
#endif

                if (_publisher._interceptorBrokers.HasInterceptors)
                {
                    var publisher = InterceptablePublisher<TScope, TMessage>.Rent();
                    publisher.scope = Scope;
                    publisher.message = message;
                    publisher.context = context;
                    publisher.publisher = this;

                    _publisher._interceptorBrokers.GetInterceptors<TScope, TMessage>(
                          publisher.Interceptors
                        , publisher.ObjectStack
                    );

                    return ContinueAsync(publisher.PublishAsync(), publisher);
                }
                else
                {
                    return PublishAsyncInternal(Scope, message, context);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static async UnityTask ContinueAsync(
                      UnityTask task
                    , InterceptablePublisher<TScope, TMessage> publisher
                )
                {
                    try
                    {
                        await task;
                    }
                    finally
                    {
                        publisher.Return();
                    }
                }
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            internal UnityTask PublishCoreAsync<TMessage>(TScope scope, TMessage message, PublishingContext context)
            {
#if __ENCOSY_VALIDATION__
                if (Validate(message, context.Logger) == false)
                {
                    return UnityTasks.GetCompleted();
                }
#endif

                return PublishAsyncInternal(scope, message, context);
            }

            private UnityTask PublishAsyncInternal<TMessage>(TScope scope, TMessage message, PublishingContext context)
            {
                if (TryGetMessageBroker<TMessage>(_publisher, out var messageBroker))
                {
                    return messageBroker.PublishAsync(scope, message, context);
                }
                else if (context.Strategy == PublishingStrategy.WaitForSubscriber)
                {
                    return WaitThenPublishAsync(_publisher, scope, message, context);
                }
#if __ENCOSY_VALIDATION__
                else
                {
                    LogWarningNoSubscriber<TMessage>(scope, context);
                }
#endif

                return UnityTasks.GetCompleted();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool TryGetMessageBroker<TMessage>(
                  MessagePublisher publisher
                , out MessageBroker<TScope, TMessage> messageBroker
            )
            {
                return publisher._messageBrokers.TryGet(out messageBroker);
            }

            private static async UnityTask WaitThenPublishAsync<TMessage>(
                  MessagePublisher publisher
                , TScope scope
                , TMessage message
                , PublishingContext context
            )
            {
                await UnityTasks.WaitUntil(
                      publisher
                    , static x => x._messageBrokers.Contains<MessageBroker<TScope, TMessage>>()
                    , context.Token
                );

                if (context.Token.IsCancellationRequested)
                {
                    return;
                }

                if (TryGetMessageBroker<TMessage>(publisher, out var broker))
                {
                    await broker.PublishAsync(scope, message, context);
                }
#if __ENCOSY_VALIDATION__
                else
                {
                    LogErrorFailedWaitThenPublishAsync<TMessage>(scope, context);
                }
#endif
            }

            private static void LogWarningNoSubscriber<TMessage>(TScope scope, PublishingContext context)
            {
                if (context.WarnNoSubscriber)
                {
                    context.Logger.LogWarning(
                        $"Found no subscription for `{typeof(TMessage)}` in scope `{scope}`"
                    );
                }
            }

            private static void LogErrorFailedWaitThenPublishAsync<TMessage>(TScope scope, PublishingContext context)
            {
                context.Logger.LogError(
                    $"Failed to wait then publish: No subscriber for message type `{typeof(TMessage)}` " +
                    $"in scope `{scope}`. This should never happen!"
                );
            }
        }
    }
}

#endif
