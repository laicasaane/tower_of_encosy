#if UNITASK

#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __ENCOSY_PUBSUB_NO_VALIDATION__
#else
#define __ENCOSY_PUBSUB_VALIDATION__
#endif

using System.Threading;
using Cysharp.Threading.Tasks;
using EncosyTower.Modules.Logging;

namespace EncosyTower.Modules.PubSub
{
    partial class MessagePublisher
    {
        partial struct UnityPublisher<TScope>
        {
#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public UniTask PublishAsync<TMessage>(
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
#if __ENCOSY_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return UniTask.CompletedTask;
                }
#endif

                return _publisher.PublishAsync<TMessage>(token, logger, callerInfo);
            }

#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public UniTask PublishAsync<TMessage>(
                  TMessage message
                , CancellationToken token = default
                , ILogger logger = null
                , CallerInfo callerInfo = default
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return UniTask.CompletedTask;
                }
#endif

                return _publisher.PublishAsync(message, token, logger, callerInfo);
            }
        }
    }
}

#endif
