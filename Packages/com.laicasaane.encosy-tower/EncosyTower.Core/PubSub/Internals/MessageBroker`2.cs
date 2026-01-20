#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Logging;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub.Internals
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    internal sealed class MessageBroker<TScope, TMessage> : IMessageBroker
    {
        private readonly ArrayMap<TScope, MessageBroker<TMessage>> _scopedBrokers = new();
        private readonly FasterList<TScope> _scopesToRemove = new();

        public bool IsEmpty => _scopedBrokers.Count <= 0;

        public void Dispose()
        {
            lock (_scopedBrokers)
            {
                var scopedBrokers = _scopedBrokers;

                var brokers = scopedBrokers.GetValues();

                foreach (var broker in brokers)
                {
                    broker?.Dispose();
                }

                scopedBrokers.Dispose();
            }
        }

        public void Compress(ILogger logger)
        {
            lock (_scopedBrokers)
            {
#if !__ENCOSY_NO_VALIDATION__
                try
#endif
                {
                    var scopedBrokers = _scopedBrokers;
                    var scopesToRemove = _scopesToRemove;

                    scopesToRemove.IncreaseCapacityTo(scopedBrokers.Count);

                    foreach (var (key, broker) in scopedBrokers)
                    {
                        broker.Compress(logger);

                        if (broker.IsEmpty)
                        {
                            broker.Dispose();
                            scopesToRemove.Add(key);
                        }
                    }

                    var scopes = scopesToRemove.AsSpan();

                    foreach (var scope in scopes)
                    {
                        scopedBrokers.Remove(scope);
                    }
                }
#if !__ENCOSY_NO_VALIDATION__
                catch (Exception ex)
                {
                    logger.LogException(ex);
                }
#endif
            }
        }

        /// <summary>
        /// Remove empty handler groups to optimize performance.
        /// </summary>
        public void Compress(TScope scope, ILogger logger)
        {
            lock (_scopedBrokers)
            {
                var scopedBrokers = _scopedBrokers;

                if (scopedBrokers.TryGetValue(scope, out var broker) == false)
                {
                    return;
                }

                if (broker.IsCached)
                {
                    return;
                }

                broker.Compress(logger);

                if (broker.IsEmpty)
                {
                    scopedBrokers.Remove(scope);
                    broker.Dispose();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityTask PublishAsync(TScope scope, TMessage message, PublishingContext context)
        {
            return _scopedBrokers.TryGetValue(scope, out var broker)
                ? broker.PublishAsync(message, context)
                : UnityTasks.GetCompleted();
        }

        public Subscription<TMessage> Subscribe(
              TScope scope
            , IHandler<TMessage> handler
            , int order
            , ArrayPool<UnityTask> taskArrayPool
            , ILogger logger
        )
        {
            lock (_scopedBrokers)
            {
                var scopedBrokers = _scopedBrokers;

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
            lock (_scopedBrokers)
            {
                var scopedBrokers = _scopedBrokers;

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
