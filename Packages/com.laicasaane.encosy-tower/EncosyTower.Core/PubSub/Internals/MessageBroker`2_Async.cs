#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Buffers;
using System.Threading;
using EncosyTower.Logging;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub.Internals
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    internal sealed partial class MessageBroker<TScope, TMessage>
    {
        public UnityTask PublishAsync(
              TScope scope
            , TMessage message
            , PublishingContext context
            , CancellationToken token
        )
        {
            var scopedBrokers = _scopedBrokers;

            lock (scopedBrokers)
            {
                return scopedBrokers.TryGetValue(scope, out var broker)
                    ? broker.PublishAsync(message, context, token)
                    : UnityTasks.GetCompleted();
            }
        }

        public Subscription<TMessage> Subscribe(
              TScope scope
            , IHandler<TMessage> handler
            , int order
            , ArrayPool<UnityTask> taskArrayPool
            , ILogger logger
        )
        {
            var scopedBrokers = _scopedBrokers;

            lock (scopedBrokers)
            {
                if (scopedBrokers.TryGetValue(scope, out var broker) == false)
                {
                    scopedBrokers[scope] = broker = new MessageBroker<TMessage>();
                    broker.TaskArrayPool = taskArrayPool;
                }

                return broker.Subscribe(handler, order, logger);
            }
        }

        public MessageBroker<TMessage> Cache(TScope scope, ArrayPool<UnityTask> taskArrayPool)
        {
            var scopedBrokers = _scopedBrokers;

            lock (scopedBrokers)
            {
                if (scopedBrokers.TryGetValue(scope, out var broker) == false)
                {
                    scopedBrokers[scope] = broker = new MessageBroker<TMessage>();
                    broker.TaskArrayPool = taskArrayPool;
                }

                broker.OnCache();
                return broker;
            }
        }
    }
}

#endif
