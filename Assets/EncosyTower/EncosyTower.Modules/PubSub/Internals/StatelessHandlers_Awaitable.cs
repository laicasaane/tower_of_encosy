#if !UNITASK && UNITY_6000_0_OR_NEWER

using System;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Modules.PubSub.Internals
{
    internal sealed class HandlerFuncMessageToken<TMessage> : IHandler<TMessage>
    {
        private Func<TMessage, CancellationToken, Awaitable> _handler;

        public HandlerFuncMessageToken(Func<TMessage, CancellationToken, Awaitable> handler)
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

            return _handler?.Invoke(message, token) ?? Awaitables.GetCompleted();
        }
    }

    internal sealed class HandlerFuncToken<TMessage> : IHandler<TMessage>
    {
        private Func<CancellationToken, Awaitable> _handler;

        public HandlerFuncToken(Func<CancellationToken, Awaitable> handler)
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

            return _handler?.Invoke(token) ?? Awaitables.GetCompleted();
        }
    }

    internal sealed class HandlerFuncMessage<TMessage> : IHandler<TMessage>
    {
        private Func<TMessage, Awaitable> _handler;

        public HandlerFuncMessage(Func<TMessage, Awaitable> handler)
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

            return _handler?.Invoke(message) ?? Awaitables.GetCompleted();
        }
    }

    internal sealed class HandlerFunc<TMessage> : IHandler<TMessage>
    {
        private Func<Awaitable> _handler;

        public HandlerFunc(Func<Awaitable> handler)
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

            return _handler?.Invoke() ?? Awaitables.GetCompleted();
        }
    }

    internal sealed class HandlerActionMessage<TMessage> : IHandler<TMessage>
    {
        private Action<TMessage> _handler;

        public HandlerActionMessage(Action<TMessage> handler)
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

            _handler?.Invoke(message);
            return Awaitables.GetCompleted();
        }
    }

    internal sealed class HandlerAction<TMessage> : IHandler<TMessage>
    {
        private Action _handler;

        public HandlerAction(Action handler)
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

            _handler?.Invoke();
            return Awaitables.GetCompleted();
        }
    }
}

#endif
