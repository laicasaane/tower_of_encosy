#if UNITASK

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using EncosyTower.Modules.Logging;

namespace EncosyTower.Modules.PubSub
{
    partial struct CachedPublisher<TMessage>
    {
#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public readonly void Publish(
              TMessage message
            , CancellationToken token = default
            , ILogger logger = null
            , CallerInfo callerInfo = default
        )
        {
#if __ENCOSY_VALIDATION__
            if (Validate(message, logger) == false)
            {
                return;
            }
#endif

            _broker.PublishAsync(message, new(callerInfo), token, logger ?? DevLogger.Default).Forget();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UniTask PublishAsync(
              CancellationToken token = default
            , ILogger logger = null
            , CallerInfo callerInfo = default
        )
        {
            return PublishAsync(new TMessage(), token, logger, callerInfo);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public readonly UniTask PublishAsync(
              TMessage message
            , CancellationToken token = default
            , ILogger logger = null
            , CallerInfo callerInfo = default
        )
        {
#if __ENCOSY_VALIDATION__
            if (Validate(message, logger) == false)
            {
                return UniTask.CompletedTask;
            }
#endif

            return _broker.PublishAsync(message, new(callerInfo), token, logger ?? DevLogger.Default);
        }
    }
}

#endif
