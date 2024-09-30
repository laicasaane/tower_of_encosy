#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __MODULE_CORE_PUBSUB_NO_VALIDATION__
#else
#define __MODULE_CORE_PUBSUB_VALIDATION__
#endif

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Module.Core.Logging;
using Module.Core.PubSub.Internals;

namespace Module.Core.PubSub
{
    public partial struct CachedPublisher<TMessage> : IDisposable
#if MODULE_CORE_PUBSUB_RELAX_MODE
        where TMessage : new()
#else
        where TMessage : IMessage, new()
#endif
    {
        internal MessageBroker<TMessage> _broker;

        internal CachedPublisher(MessageBroker<TMessage> broker)
        {
            _broker = broker ?? throw new System.ArgumentNullException(nameof(broker));
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

#if __MODULE_CORE_PUBSUB_VALIDATION__
        private readonly bool Validate(ILogger logger)
        {
            if (_broker == null)
            {
                (logger ?? DevLogger.Default).LogError(
                    $"{GetType()} must be retrieved via `{nameof(MessagePublisher)}.{nameof(MessagePublisher.Cache)}` API"
                );

                return false;
            }

            return true;
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

            if (message == null)
            {
                (logger ?? DevLogger.Default).LogException(new System.ArgumentNullException(nameof(message)));
                return false;
            }

            return true;
        }
#endif
    }
}

#endif
