#if UNITASK

#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __MODULE_CORE_PUBSUB_NO_VALIDATION__
#else
#define __MODULE_CORE_PUBSUB_VALIDATION__
#endif

using System.Threading;
using Cysharp.Threading.Tasks;
using Module.Core.Logging;

namespace Module.Core.PubSub
{
    partial class AnonPublisher
    {
        partial struct Publisher<TScope>
        {
#if __MODULE_CORE_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public UniTask PublishAsync(
                  CancellationToken token = default
                , ILogger logger = null
                , CallerInfo callerInfo = default
            )
            {
#if __MODULE_CORE_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return UniTask.CompletedTask;
                }
#endif

                return _publisher.PublishAsync<AnonMessage>(default, token, logger, callerInfo);
            }
        }
    }
}

#endif
