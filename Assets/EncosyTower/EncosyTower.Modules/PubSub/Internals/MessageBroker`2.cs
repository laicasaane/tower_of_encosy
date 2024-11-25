#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using EncosyTower.Modules.Collections;
using EncosyTower.Modules.Logging;

namespace EncosyTower.Modules.PubSub.Internals
{
    internal sealed partial class MessageBroker<TScope, TMessage> : MessageBroker
    {
        private readonly ArrayMap<TScope, MessageBroker<TMessage>> _scopedBrokers = new();
        private readonly FasterList<TScope> _scopesToRemove = new();

        public bool IsEmpty => _scopedBrokers.Count <= 0;

        public override void Dispose()
        {
            var scopedBrokers = _scopedBrokers;

            lock (scopedBrokers)
            {
                var brokers = scopedBrokers.GetValues();

                foreach (var broker in brokers)
                {
                    broker?.Dispose();
                }

                scopedBrokers.Dispose();
            }
        }

        public override void Compress(ILogger logger)
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
            var scopedBrokers = _scopedBrokers;

            lock (scopedBrokers)
            {
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
    }
}

#endif
