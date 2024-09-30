#if !UNITASK && UNITY_6000_0_OR_NEWER

using System.Threading;
using Module.Core.Collections;
using UnityEngine;

namespace Module.Core.PubSub.Internals
{
    internal sealed partial class MessageBroker<TScope, TMessage> : MessageBroker
    {
        public Awaitable PublishAsync(
              TScope scope, TMessage message
            , PublishingContext context
            , CancellationToken token
            , Module.Core.Logging.ILogger logger
        )
        {
            var scopedBrokers = _scopedBrokers;

            lock (scopedBrokers)
            {
                if (scopedBrokers.TryGetValue(scope, out var broker))
                {
                    return broker.PublishAsync(message, context, token, logger);
                }

                return Awaitables.GetCompleted();
            }
        }

        public Subscription<TMessage> Subscribe(
              TScope scope
            , IHandler<TMessage> handler
            , int order
            , CappedArrayPool<Awaitable> taskArrayPool
            , Module.Core.Logging.ILogger logger
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
