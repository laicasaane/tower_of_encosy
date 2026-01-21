#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Initialization;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub.Internals
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    internal class InterceptablePublisher<TScope, TMessage> : IInitializable, IDeinitializable
#if !ENCOSY_PUBSUB_RELAX_MODE
        where TMessage : IMessage
#endif
    {
        public readonly FasterList<object> Interceptors = new();
        public readonly Stack<object> ObjectStack = new();

        private readonly PublishContinuation<TScope, TMessage> _scopedContinuation;
        private readonly PublishContinuation<TMessage> _continuation;

        public TScope scope;
        public TMessage message;
        public PublishingContext context;
        public MessagePublisher.Publisher<TScope> publisher;
        public CachedPublisher<TScope, TMessage> cachedPublisher;

        private int _currentInterceptorIndex;

        public InterceptablePublisher()
        {
            _scopedContinuation = PublishRecursiveAsync;
            _continuation = PublishRecursiveAsync;
        }

        public static InterceptablePublisher<TScope, TMessage> Rent()
        {
            return InterceptablePublisherPool<TScope, TMessage>.Rent();
        }

        public void Initialize()
        {
            Deinitialize();
        }

        public void Deinitialize()
        {
            scope = default;
            message = default;
            context = default;
            publisher = default;
            cachedPublisher = default;

            Interceptors.Clear();
            ObjectStack.Clear();
        }

        public void Return()
        {
            InterceptablePublisherPool<TScope, TMessage>.Return(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityTask PublishAsync()
        {
            _currentInterceptorIndex = -1;

            return PublishRecursiveAsync(scope, message, context);
        }

        private UnityTask PublishRecursiveAsync(TScope scope, TMessage message, PublishingContext context)
        {
            this.scope = scope;
            this.message = message;
            this.context = context;

            if (MoveNextInterceptor(out var interceptor)
                && TryInterceptAsync(interceptor).TryGetValue(out var task)
            )
            {
                return task;
            }

            if (publisher.IsCreated)
            {
                return publisher.PublishCoreAsync(this.scope, this.message, this.context);
            }

            if (cachedPublisher.IsCreated)
            {
                return cachedPublisher.PublishCoreAsync(this.scope, this.message, this.context);
            }

            return UnityTasks.GetCompleted();
        }

        private UnityTask PublishRecursiveAsync(TMessage message, PublishingContext context)
        {
            this.message = message;
            this.context = context;

            if (MoveNextInterceptor(out var interceptor)
                && TryInterceptAsync(interceptor).TryGetValue(out var task)
            )
            {
                return task;
            }

            if (publisher.IsCreated)
            {
                return publisher.PublishCoreAsync(this.scope, this.message, this.context);
            }

            if (cachedPublisher.IsCreated)
            {
                return cachedPublisher.PublishCoreAsync(this.scope, this.message, this.context);
            }

            return UnityTasks.GetCompleted();
        }

        private bool MoveNextInterceptor(out object nextInterceptor)
        {
            var interceptors = Interceptors.AsSpan();

            while (++_currentInterceptorIndex < interceptors.Length)
            {
                if (interceptors[_currentInterceptorIndex] is { } interceptor)
                {
                    nextInterceptor = interceptor;
                    return true;
                }
            }

            nextInterceptor = default;
            return false;
        }

        private Option<UnityTask> TryInterceptAsync(object obj)
        {
            switch (obj)
            {
                case IScopedMessageInterceptor<TScope, TMessage> i:
                    return i.InterceptAsync(scope, message, context, _scopedContinuation);

                case IMessageInterceptor<TMessage> i:
                    return i.InterceptAsync(message, context, _continuation);

                case IScopedMessageInterceptor i:
                    return i.InterceptAsync(scope, message, context, _scopedContinuation);

                case IMessageInterceptor i:
                    return i.InterceptAsync(message, context, _continuation);

                default:
                    return Option.None;
            }
        }
    }
}

#endif
