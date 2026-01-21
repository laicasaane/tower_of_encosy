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
using EncosyTower.Common;
using EncosyTower.Logging;

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

        public bool HasHandlers
        {
            get
            {
                lock (_scopedBrokers)
                {
                    var scopedBrokers = _scopedBrokers;

                    foreach (var (_, broker) in scopedBrokers)
                    {
                        if (broker.HasHandlers)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
        }

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

                        if (broker.HasHandlers == false)
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

                if (broker.HasHandlers == false)
                {
                    scopedBrokers.Remove(scope);
                    broker.Dispose();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<UnityTask> TryPublishAsync(TScope scope, TMessage message, PublishingContext context)
        {
            return _scopedBrokers.TryGetValue(scope, out var broker)
                ? broker.TryPublishAsync(message, context)
                : Option.None;
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
