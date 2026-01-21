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
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public partial struct CachedPublisher<TScope, TMessage> : IDisposable, IIsCreated
#if ENCOSY_PUBSUB_RELAX_MODE
        where TMessage
#else
        where TMessage : IMessage
#endif
    {
        internal MessageBroker<TMessage> _broker;
        internal InterceptorBrokers _interceptorBrokers;
        internal Func<TMessage> _createFunc;
        internal TScope _scope;

        internal CachedPublisher(
              [NotNull] MessageBroker<TMessage> broker
            , [NotNull] InterceptorBrokers interceptorBrokers
            , [NotNull] Func<TMessage> createFunc
            , TScope scope
        )
        {
            _broker = broker;
            _interceptorBrokers = interceptorBrokers;
            _createFunc = createFunc;
            _scope = scope;
        }

        public readonly bool IsCreated => _broker != null;

        public readonly MessageInterceptors Interceptors => new(_interceptorBrokers);

        public void Dispose()
        {
            _broker?.OnUncache();
            _broker = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Publish(PublishingContext context = default)
        {
#if __ENCOSY_VALIDATION__
            if (Validate(context.Logger) == false)
            {
                return;
            }
#endif

            Publish(_createFunc(), context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Publish(TMessage message, PublishingContext context = default)
        {
            PublishAsync(message, context).Forget();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnityTask PublishAsync(PublishingContext context = default)
        {
            return PublishAsync(_createFunc(), context);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public readonly UnityTask PublishAsync(TMessage message, PublishingContext context = default)
        {
#if __ENCOSY_VALIDATION__
            if (Validate(message, context.Logger) == false)
            {
                return UnityTasks.GetCompleted();
            }
#endif

            if (_interceptorBrokers.HasInterceptors)
            {
                var publisher = InterceptablePublisher<TScope, TMessage>.Rent();
                publisher.scope = _scope;
                publisher.message = message;
                publisher.context = context;
                publisher.cachedPublisher = this;

                _interceptorBrokers.GetInterceptors<TScope, TMessage>(
                      publisher.Interceptors
                    , publisher.ObjectStack
                );

                return ContinueAsync(publisher.PublishAsync(), publisher);
            }
            else
            {
                return PublishAsyncInternal(_scope, message, context);
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
        internal readonly UnityTask PublishCoreAsync(TScope scope, TMessage message, PublishingContext context)
        {
#if __ENCOSY_VALIDATION__
            if (Validate(message, context.Logger) == false)
            {
                return UnityTasks.GetCompleted();
            }
#endif

            return PublishAsyncInternal(scope, message, context);
        }

        private readonly UnityTask PublishAsyncInternal(TScope scope, TMessage message, PublishingContext context)
        {
            if (_broker.TryPublishAsync(message, context).TryGetValue(out var task))
            {
                return task;
            }

            if (context.Strategy == PublishingStrategy.WaitForSubscriber)
            {
                return WaitThenPublishAsync(message, context);
            }
#if __ENCOSY_VALIDATION__
            else
            {
                LogWarningNoSubscriber(scope, context);
            }
#endif

            return UnityTasks.GetCompleted();
        }

        private readonly async UnityTask WaitThenPublishAsync(TMessage message, PublishingContext context)
        {
            await UnityTasks.WaitUntil(_broker, static x => x.HasHandlers, context.Token);

            if (context.Token.IsCancellationRequested)
            {
                return;
            }

            if (_broker.TryPublishAsync(message, context).TryGetValue(out var task))
            {
                await task;
                return;
            }

#if __ENCOSY_VALIDATION__

            {
                LogErrorFailedWaitThenPublishAsync(_scope, context);
            }
#endif
        }

#if __ENCOSY_VALIDATION__
        private readonly bool Validate(ILogger logger)
        {
            if (_broker == null)
            {
                (logger ?? DevLogger.Default).LogError(
                    $"{GetType()} must be retrieved via `{nameof(MessagePublisher)}.{nameof(MessagePublisher.Cache)}` API"
                );

                return false;
            }

            return false;
        }

        private readonly bool Validate(TMessage message, ILogger logger)
        {
            if (_broker == null)
            {
                (logger ?? DevLogger.Default).LogError(
                    $"{GetType()} must be retrieved via `{nameof(MessagePublisher)}.{nameof(MessagePublisher.Cache)}` API"
                );

                return false;
            }

            if (message != null)
            {
                return true;
            }

            (logger ?? DevLogger.Default).LogException(new ArgumentNullException(nameof(message)));
            return false;
        }
#endif

        private static void LogWarningNoSubscriber(TScope scope, PublishingContext context)
        {
            if (context.WarnNoSubscriber)
            {
                context.Logger.LogWarning(
                    $"Found no subscription for `{typeof(TMessage)}` in scope `{scope}`"
                );
            }
        }

        private static void LogErrorFailedWaitThenPublishAsync(TScope scope, PublishingContext context)
        {
            context.Logger.LogError(
                $"Failed to wait then publish: No subscriber for message type `{typeof(TMessage)}` " +
                $"in scope `{scope}`. This should never happen!"
            );
        }
    }
}

#endif
