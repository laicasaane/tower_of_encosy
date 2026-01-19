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

                if (TryGetBroker<TMessage>(_publisher, out var broker))
                {
                    return broker.PublishAsync(Scope, message, context);
                }
                else if (context.Strategy == PublishingStrategy.WaitForSubscriber)
                {
                    return LatePublishAsync(_publisher, Scope, message, context);
                }
#if __ENCOSY_VALIDATION__
                else
                {
                    LogWarningNoSubscriber<TMessage>(Scope, context);
                }
#endif

                return UnityTasks.GetCompleted();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void RetainUsings_UniTask();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool TryGetBroker<TMessage>(
                  MessagePublisher publisher
                , out MessageBroker<TScope, TMessage> broker
            )
            {
                return publisher._brokers.TryGet(out broker);
            }

            private static async UnityTask LatePublishAsync<TMessage>(
                  MessagePublisher publisher
                , TScope scope
                , TMessage message
                , PublishingContext context
            )
            {
                await UnityTasks.WaitUntil(
                      publisher
                    , static x => x._brokers.Contains<MessageBroker<TScope, TMessage>>()
                    , context.Token
                );

                if (context.Token.IsCancellationRequested)
                {
                    return;
                }

                if (TryGetBroker<TMessage>(publisher, out var broker))
                {
                    await broker.PublishAsync(scope, message, context);
                }
#if __ENCOSY_VALIDATION__
                else
                {
                    LogErrorFailedLatePublishAsync<TMessage>(scope, context);
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

            private static void LogErrorFailedLatePublishAsync<TMessage>(TScope scope, PublishingContext context)
            {
                context.Logger.LogError(
                    $"Failed late publish: No subscriber for message type `{typeof(TMessage)}` in scope `{scope}`. " +
                    $"This should never happen!"
                );
            }
        }
    }
}

#endif
