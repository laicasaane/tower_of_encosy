#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Modules.Logging;

namespace EncosyTower.Modules.PubSub
{
    partial class MessagePublisher
    {
        public readonly partial struct UnityPublisher<TScope>
            where TScope : UnityEngine.Object
        {
            internal readonly Publisher<UnityInstanceId<TScope>> _publisher;

            public bool IsValid => _publisher.IsValid;

            public UnityInstanceId<TScope> Scope => _publisher.Scope;

            internal UnityPublisher([NotNull] MessagePublisher publisher, [NotNull] TScope scope)
            {
                _publisher = new(publisher, scope);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public CachedPublisher<TMessage> Cache<TMessage>(ILogger logger = null)
#if ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : new()
#else
                where TMessage : IMessage, new()
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return default;
                }
#endif

                return _publisher.Cache<TMessage>(logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Publish<TMessage>(
                  CancellationToken token = default
                , ILogger logger = null
                , CallerInfo callerInfo = default
            )
#if ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : new()
#else
                where TMessage : IMessage, new()
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _publisher.Publish<TMessage>(token, logger, callerInfo);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Publish<TMessage>(
                  TMessage message
                , CancellationToken token = default
                , ILogger logger = null
                , CallerInfo callerInfo = default
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _publisher.Publish(message, token, logger, callerInfo);
            }

#if __ENCOSY_VALIDATION__
            private bool Validate(ILogger logger)
            {
                if (IsValid)
                {
                    return true;
                }

                (logger ?? DevLogger.Default).LogError(
                    $"{GetType().Name} must be retrieved via `{nameof(MessagePublisher)}.{nameof(UnityScope)}` API"
                );

                return false;
            }
#endif

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void RetainUsings();
        }
    }
}

#endif
