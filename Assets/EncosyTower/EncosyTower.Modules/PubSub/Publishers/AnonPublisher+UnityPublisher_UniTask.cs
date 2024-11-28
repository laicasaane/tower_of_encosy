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
    partial class AnonPublisher
    {
        partial struct UnityPublisher<TScope>
        {
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public UniTask PublishAsync(
                  CancellationToken token = default
                , ILogger logger = null
                , CallerInfo callerInfo = default
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return UniTask.CompletedTask;
                }
#endif

                return _publisher.PublishAsync<AnonMessage>(default, token, logger, callerInfo);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void RetainUsings_UniTask();
        }
    }
}

#endif
