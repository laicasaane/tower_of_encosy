#if UNITASK

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace EncosyTower.Modules.PubSub.Internals
{
    internal sealed class HandlerFuncMessageToken<TMessage> : IHandler<TMessage>
    {
        private Func<TMessage, CancellationToken, UniTask> _handler;

        public HandlerFuncMessageToken(Func<TMessage, CancellationToken, UniTask> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public UniTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UniTask.CompletedTask;
            }

            return _handler?.Invoke(message, token) ?? UniTask.CompletedTask;
        }
    }

    internal sealed class HandlerFuncToken<TMessage> : IHandler<TMessage>
    {
        private Func<CancellationToken, UniTask> _handler;

        public HandlerFuncToken(Func<CancellationToken, UniTask> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public UniTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UniTask.CompletedTask;
            }

            return _handler?.Invoke(token) ?? UniTask.CompletedTask;
        }
    }

    internal sealed class HandlerFuncMessage<TMessage> : IHandler<TMessage>
    {
        private Func<TMessage, UniTask> _handler;

        public HandlerFuncMessage(Func<TMessage, UniTask> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public UniTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UniTask.CompletedTask;
            }

            return _handler?.Invoke(message) ?? UniTask.CompletedTask;
        }
    }

    internal sealed class HandlerFunc<TMessage> : IHandler<TMessage>
    {
        private Func<UniTask> _handler;

        public HandlerFunc(Func<UniTask> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public UniTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UniTask.CompletedTask;
            }

            return _handler?.Invoke() ?? UniTask.CompletedTask;
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

        public UniTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UniTask.CompletedTask;
            }

            _handler?.Invoke(message);
            return UniTask.CompletedTask;
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

        public UniTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UniTask.CompletedTask;
            }

            _handler?.Invoke();
            return UniTask.CompletedTask;
        }
    }
}

#endif
