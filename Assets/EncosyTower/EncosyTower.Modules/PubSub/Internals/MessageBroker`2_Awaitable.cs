#if !UNITASK && UNITY_6000_0_OR_NEWER

using System.Threading;
using EncosyTower.Modules.Collections;
using UnityEngine;

namespace EncosyTower.Modules.PubSub.Internals
{
    internal sealed partial class MessageBroker<TScope, TMessage>
    {
        public Awaitable PublishAsync(
              TScope scope, TMessage message
            , PublishingContext context
            , CancellationToken token
            , EncosyTower.Modules.Logging.ILogger logger
        )
        {
            var scopedBrokers = _scopedBrokers;

            lock (scopedBrokers)
            {
                return scopedBrokers.TryGetValue(scope, out var broker)
                    ? broker.PublishAsync(message, context, token, logger)
                    : Awaitables.GetCompleted();
            }
        }

        public Subscription<TMessage> Subscribe(
              TScope scope
            , IHandler<TMessage> handler
            , int order
            , CappedArrayPool<Awaitable> taskArrayPool
            , EncosyTower.Modules.Logging.ILogger logger
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

        public MessageBroker<TMessage> Cache(TScope scope, CappedArrayPool<Awaitable> taskArrayPool)
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
