#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Modules.Logging;
using EncosyTower.Modules.PubSub.Internals;

namespace EncosyTower.Modules.PubSub
{
    public partial struct CachedPublisher<TMessage> : IDisposable
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

        public readonly bool IsValid => _broker != null;

        public void Dispose()
        {
            _broker?.OnUncache();
            _broker = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Publish(
              CancellationToken token = default
            , ILogger logger = null
            , CallerInfo callerInfo = default
        )
        {
            Publish(new TMessage(), token, logger, callerInfo);
        }

#if __ENCOSY_VALIDATION__
        private readonly bool Validate(ILogger logger)
        {
            if (_broker != null)
            {
                return true;
            }

            (logger ?? DevLogger.Default).LogError(
                $"{GetType()} must be retrieved via `{nameof(MessagePublisher)}.{nameof(MessagePublisher.Cache)}` API"
            );

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
    }
}

#endif
