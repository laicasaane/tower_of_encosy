#if UNITASK

using System.Threading;
using Cysharp.Threading.Tasks;
using Module.Core.Collections;
using Module.Core.Logging;

namespace Module.Core.PubSub.Internals
{
    internal sealed partial class MessageBroker<TScope, TMessage> : MessageBroker
    {
        public UniTask PublishAsync(
              TScope scope, TMessage message
            , PublishingContext context
            , CancellationToken token
            , ILogger logger
        )
        {
            var scopedBrokers = _scopedBrokers;

            lock (scopedBrokers)
            {
                if (scopedBrokers.TryGetValue(scope, out var broker))
                {
                    return broker.PublishAsync(message, context, token, logger);
                }

                return UniTask.CompletedTask;
            }
        }

        public Subscription<TMessage> Subscribe(
              TScope scope
            , IHandler<TMessage> handler
            , int order
            , CappedArrayPool<UniTask> taskArrayPool
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

        public MessageBroker<TMessage> Cache(TScope scope, CappedArrayPool<UniTask> taskArrayPool)
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
