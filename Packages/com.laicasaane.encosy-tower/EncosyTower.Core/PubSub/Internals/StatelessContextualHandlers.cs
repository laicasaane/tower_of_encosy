#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using EncosyTower.Common;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub.Internals
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    internal sealed class ContextualHandlerFuncMessage<TMessage> : IHandler<TMessage>
    {
        private Func<TMessage, PublishingContext, UnityTask> _handler;

        public ContextualHandlerFuncMessage(Func<TMessage, PublishingContext, UnityTask> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public UnityTask Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested)
            {
                return UnityTasks.GetCompleted();
            }

            return _handler?.Invoke(message, context) ?? UnityTasks.GetCompleted();
        }
    }

    internal sealed class ContextualHandlerFunc<TMessage> : IHandler<TMessage>
    {
        private Func<PublishingContext, UnityTask> _handler;

        public ContextualHandlerFunc(Func<PublishingContext, UnityTask> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public UnityTask Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested)
            {
                return UnityTasks.GetCompleted();
            }

            return _handler?.Invoke(context) ?? UnityTasks.GetCompleted();
        }
    }

    internal sealed class ContextualHandlerActionMessage<TMessage> : IHandler<TMessage>
    {
        private Action<TMessage, PublishingContext> _handler;

        public ContextualHandlerActionMessage(Action<TMessage, PublishingContext> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public UnityTask Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested)
            {
                return UnityTasks.GetCompleted();
            }

            _handler?.Invoke(message, context);
            return UnityTasks.GetCompleted();
        }
    }

    internal sealed class ContextualHandlerAction<TMessage> : IHandler<TMessage>
    {
        private Action<PublishingContext> _handler;

        public ContextualHandlerAction(Action<PublishingContext> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public UnityTask Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested)
            {
                return UnityTasks.GetCompleted();
            }

            _handler?.Invoke(context);
            return UnityTasks.GetCompleted();
        }
    }
}

#endif
