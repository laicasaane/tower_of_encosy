#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System.Runtime.CompilerServices;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    partial class MessagePublisher
    {
        partial struct UnityPublisher<TScope>
        {
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public UnityTask PublishAsync<TMessage>(PublishingContext context = default)
#if ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : new()
#else
                where TMessage : IMessage, new()
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(context.Logger) == false)
                {
                    return UnityTasks.GetCompleted();
                }
#endif

                return _publisher.PublishAsync<TMessage>(context);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public UnityTask PublishAsync<TMessage>(TMessage message, PublishingContext context = default)
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(context.Logger) == false)
                {
                    return UnityTasks.GetCompleted();
                }
#endif

                return _publisher.PublishAsync(message, context);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void RetainUsings_Async();
        }
    }
}

#endif
