#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    partial struct CachedPublisher<TMessage>
    {
#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public readonly void Publish(
              TMessage message
            , PublishingContext context = default
            , CancellationToken token = default
        )
        {
#if __ENCOSY_VALIDATION__
            if (Validate(message, context.Logger) == false)
            {
                return;
            }
#endif

            _broker.PublishAsync(message, context, token).Forget();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnityTask PublishAsync(
              PublishingContext context = default
            , CancellationToken token = default
        )
        {
            return PublishAsync(new TMessage(), context, token);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public readonly UnityTask PublishAsync(
              TMessage message
            , PublishingContext context = default
            , CancellationToken token = default
        )
        {
#if __ENCOSY_VALIDATION__
            if (Validate(message, context.Logger) == false)
            {
                return UnityTasks.GetCompleted();
            }
#endif

            return _broker.PublishAsync(message, context, token);
        }
    }
}

#endif
