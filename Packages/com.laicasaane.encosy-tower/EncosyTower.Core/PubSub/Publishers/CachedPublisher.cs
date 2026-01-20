#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
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

    public partial struct CachedPublisher<TMessage> : IDisposable, IIsCreated
#if ENCOSY_PUBSUB_RELAX_MODE
        where TMessage : new()
#else
        where TMessage : IMessage, new()
#endif
    {
        internal MessageBroker<TMessage> _broker;

        internal CachedPublisher(MessageBroker<TMessage> broker)
        {
            _broker = broker ?? throw new ArgumentNullException(nameof(broker));
        }

        public readonly bool IsCreated => _broker != null;

        public void Dispose()
        {
            _broker?.OnUncache();
            _broker = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Publish(PublishingContext context = default)
        {
            Publish(new TMessage(), context);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public readonly void Publish(TMessage message, PublishingContext context = default)
        {
#if __ENCOSY_VALIDATION__
            if (Validate(message, context.Logger) == false)
            {
                return;
            }
#endif

            _broker.PublishAsync(message, context).Forget();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnityTask PublishAsync(PublishingContext context = default)
        {
            return PublishAsync(new TMessage(), context);
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

            return _broker.PublishAsync(message, context);
        }

#if __ENCOSY_VALIDATION__
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
    }
}

#endif
