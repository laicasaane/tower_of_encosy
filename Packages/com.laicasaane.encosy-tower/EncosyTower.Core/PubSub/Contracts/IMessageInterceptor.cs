#if UNITASK || UNITY_6000_0_OR_NEWER

namespace EncosyTower.PubSub
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public delegate UnityTask PublishContinuation<TMessage>(TMessage message, PublishingContext context)
#if !ENCOSY_PUBSUB_RELAX_MODE
        where TMessage : IMessage
#endif
        ;

    public delegate UnityTask PublishContinuation<TScope, TMessage>(TScope scope, TMessage message, PublishingContext context)
#if !ENCOSY_PUBSUB_RELAX_MODE
        where TMessage : IMessage
#endif
        ;

    public interface IMessageInterceptor
    {
        UnityTask InterceptAsync<TMessage>(
              TMessage message
            , PublishingContext context
            , PublishContinuation<TMessage> continuation
        )
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
            ;
    }

    public interface IScopedMessageInterceptor
    {
        UnityTask InterceptAsync<TScope, TMessage>(
              TScope scope
            , TMessage message
            , PublishingContext context
            , PublishContinuation<TScope, TMessage> continuation
        )
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
            ;
    }

    public interface IMessageInterceptor<TMessage>
#if !ENCOSY_PUBSUB_RELAX_MODE
        where TMessage : IMessage
#endif
    {
        UnityTask InterceptAsync(
              TMessage message
            , PublishingContext context
            , PublishContinuation<TMessage> continuation
        );
    }

    public interface IScopedMessageInterceptor<TScope, TMessage>
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
    {
        UnityTask InterceptAsync(
              TScope scope
            , TMessage message
            , PublishingContext context
            , PublishContinuation<TScope, TMessage> continuation
        );
    }
}

#endif
