#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub.Internals
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    internal sealed class HandlerFuncMessageToken<TMessage> : IHandler<TMessage>
    {
        private Func<TMessage, CancellationToken, UnityTask> _handler;

        public HandlerFuncMessageToken(Func<TMessage, CancellationToken, UnityTask> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public UnityTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UnityTasks.GetCompleted();
            }

            return _handler?.Invoke(message, token) ?? UnityTasks.GetCompleted();
        }
    }

    internal sealed class HandlerFuncToken<TMessage> : IHandler<TMessage>
    {
        private Func<CancellationToken, UnityTask> _handler;

        public HandlerFuncToken(Func<CancellationToken, UnityTask> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public UnityTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UnityTasks.GetCompleted();
            }

            return _handler?.Invoke(token) ?? UnityTasks.GetCompleted();
        }
    }

    internal sealed class HandlerFuncMessage<TMessage> : IHandler<TMessage>
    {
        private Func<TMessage, UnityTask> _handler;

        public HandlerFuncMessage(Func<TMessage, UnityTask> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public UnityTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UnityTasks.GetCompleted();
            }

            return _handler?.Invoke(message) ?? UnityTasks.GetCompleted();
        }
    }

    internal sealed class HandlerFunc<TMessage> : IHandler<TMessage>
    {
        private Func<UnityTask> _handler;

        public HandlerFunc(Func<UnityTask> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public UnityTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UnityTasks.GetCompleted();
            }

            return _handler?.Invoke() ?? UnityTasks.GetCompleted();
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

        public UnityTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UnityTasks.GetCompleted();
            }

            _handler?.Invoke(message);
            return UnityTasks.GetCompleted();
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

        public UnityTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return UnityTasks.GetCompleted();
            }

            _handler?.Invoke();
            return UnityTasks.GetCompleted();
        }
    }
}

#endif
