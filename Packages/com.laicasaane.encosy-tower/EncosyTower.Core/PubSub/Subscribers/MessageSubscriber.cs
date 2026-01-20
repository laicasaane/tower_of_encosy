#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PubSub.Internals;
using EncosyTower.Vaults;

namespace EncosyTower.PubSub
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public partial class MessageSubscriber
    {
        private readonly SingletonVault<IMessageBroker> _messageBrokers;
        private readonly ArrayPool<UnityTask> _taskArrayPool;

        internal MessageSubscriber(
              SingletonVault<IMessageBroker> messageBrokers
            , ArrayPool<UnityTask> taskArrayPool
        )
        {
            _messageBrokers = messageBrokers;
            _taskArrayPool = taskArrayPool;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Subscriber<GlobalScope> Global()
        {
            return new(this, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Subscriber<TScope> Scope<TScope>()
            where TScope : struct
        {
            return new(this, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Subscriber<TScope> Scope<TScope>([NotNull] TScope scope)
        {
            return new(this, scope);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitySubscriber<TScope> UnityScope<TScope>([NotNull] TScope scope)
            where TScope : UnityEngine.Object
        {
            return new(this, scope);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Compress<TScope, TMessage>(TScope scope, ILogger logger = null)
        {
            if (_messageBrokers.TryGet<MessageBroker<TScope, TMessage>>(out var broker))
            {
                broker.Compress(scope, logger);
            }
        }

        private bool TrySubscribe<TScope, TMessage>(
              IHandler<TMessage> handler
            , int order
            , TScope scope
            , out Subscription<TMessage> subscription
            , ILogger logger
        )
        {
            lock (_messageBrokers)
            {
                var taskArrayPool = _taskArrayPool;
                var brokers = _messageBrokers;

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

                subscription = broker.Subscribe(scope, handler, order, taskArrayPool, logger);
                return true;
            }
        }
    }
}

#endif
