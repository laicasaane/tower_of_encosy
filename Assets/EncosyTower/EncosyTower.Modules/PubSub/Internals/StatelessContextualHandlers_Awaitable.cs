#if !UNITASK && UNITY_6000_0_OR_NEWER

using System;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Modules.PubSub.Internals
{
    internal sealed class ContextualHandlerFuncMessageToken<TMessage> : IHandler<TMessage>
    {
        private Func<TMessage, PublishingContext, CancellationToken, Awaitable> _handler;

        public ContextualHandlerFuncMessageToken(Func<TMessage, PublishingContext, CancellationToken, Awaitable> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public Awaitable Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return Awaitables.GetCompleted();
            }

            return _handler?.Invoke(message, context, token) ?? Awaitables.GetCompleted();
        }
    }

    internal sealed class ContextualHandlerFuncToken<TMessage> : IHandler<TMessage>
    {
        private Func<PublishingContext, CancellationToken, Awaitable> _handler;

        public ContextualHandlerFuncToken(Func<PublishingContext, CancellationToken, Awaitable> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public Awaitable Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return Awaitables.GetCompleted();
            }

            return _handler?.Invoke(context, token) ?? Awaitables.GetCompleted();
        }
    }

    internal sealed class ContextualHandlerFuncMessage<TMessage> : IHandler<TMessage>
    {
        private Func<TMessage, PublishingContext, Awaitable> _handler;

        public ContextualHandlerFuncMessage(Func<TMessage, PublishingContext, Awaitable> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public Awaitable Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return Awaitables.GetCompleted();
            }

            return _handler?.Invoke(message, context) ?? Awaitables.GetCompleted();
        }
    }

    internal sealed class ContextualHandlerFunc<TMessage> : IHandler<TMessage>
    {
        private Func<PublishingContext, Awaitable> _handler;

        public ContextualHandlerFunc(Func<PublishingContext, Awaitable> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public Awaitable Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return Awaitables.GetCompleted();
            }

            return _handler?.Invoke(context) ?? Awaitables.GetCompleted();
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

        public Awaitable Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return Awaitables.GetCompleted();
            }

            _handler?.Invoke(message, context);
            return Awaitables.GetCompleted();
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

        public Awaitable Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return Awaitables.GetCompleted();
            }

            _handler?.Invoke(context);
            return Awaitables.GetCompleted();
        }
    }
}

#endif
