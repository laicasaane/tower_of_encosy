#if !UNITASK && UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __ENCOSY_PUBSUB_NO_VALIDATION__
#else
#define __ENCOSY_PUBSUB_VALIDATION__
#endif

using System.Threading;
using UnityEngine;

namespace EncosyTower.Modules.PubSub
{
    partial class AnonPublisher
    {
        partial struct Publisher<TScope>
        {
#if __ENCOSY_PUBSUB_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public Awaitable PublishAsync(
                  CancellationToken token = default
                , EncosyTower.Modules.Logging.ILogger logger = null
                , CallerInfo callerInfo = default
            )
            {
#if __ENCOSY_PUBSUB_VALIDATION__
                if (Validate(logger) == false)
                {
                    return Awaitables.GetCompleted();
                }
#endif

                return _publisher.PublishAsync<AnonMessage>(default, token, logger, callerInfo);
            }
        }
    }
}

#endif
