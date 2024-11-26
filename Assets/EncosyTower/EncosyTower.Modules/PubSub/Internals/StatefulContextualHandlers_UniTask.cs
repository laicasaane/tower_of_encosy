#if UNITASK

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace EncosyTower.Modules.PubSub.Internals
{
    internal sealed class StatefulContextualHandlerFuncMessageToken<TState, TMessage> : IHandler<TMessage>
        where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, TMessage, PublishingContext, CancellationToken, UniTask> _handler;

        public StatefulContextualHandlerFuncMessageToken(TState state, Func<TState, TMessage, PublishingContext, CancellationToken, UniTask> handler)
        {
            _state = new(state ?? throw new ArgumentNullException(nameof(state)));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler, state.GetHashCode());
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
            _state = null;
        }

        public UniTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return UniTask.CompletedTask;
            }

            return _handler?.Invoke(state, message, context, token) ?? UniTask.CompletedTask;
        }
    }

    internal sealed class StatefulContextualHandlerFuncToken<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, PublishingContext, CancellationToken, UniTask> _handler;

        public StatefulContextualHandlerFuncToken(TState state, Func<TState, PublishingContext, CancellationToken, UniTask> handler)
        {
            _state = new(state ?? throw new ArgumentNullException(nameof(state)));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler, state.GetHashCode());
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
            _state = null;
        }

        public UniTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return UniTask.CompletedTask;
            }

            return _handler?.Invoke(state, context, token) ?? UniTask.CompletedTask;
        }
    }

    internal sealed class StatefulContextualHandlerFuncMessage<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, TMessage, PublishingContext, UniTask> _handler;

        public StatefulContextualHandlerFuncMessage(TState state, Func<TState, TMessage, PublishingContext, UniTask> handler)
        {
            _state = new(state ?? throw new ArgumentNullException(nameof(state)));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler, state.GetHashCode());
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
            _state = null;
        }

        public UniTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return UniTask.CompletedTask;
            }

            return _handler?.Invoke(state, message, context) ?? UniTask.CompletedTask;
        }
    }

    internal sealed class StatefulContextualHandlerFunc<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, PublishingContext, UniTask> _handler;

        public StatefulContextualHandlerFunc(TState state, Func<TState, PublishingContext, UniTask> handler)
        {
            _state = new(state ?? throw new ArgumentNullException(nameof(state)));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler, state.GetHashCode());
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
            _state = null;
        }

        public UniTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return UniTask.CompletedTask;
            }

            return _handler?.Invoke(state, context) ?? UniTask.CompletedTask;
        }
    }

    internal sealed class StatefulContextualHandlerActionMessage<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Action<TState, TMessage, PublishingContext> _handler;

        public StatefulContextualHandlerActionMessage(TState state, Action<TState, TMessage, PublishingContext> handler)
        {
            _state = new(state ?? throw new ArgumentNullException(nameof(state)));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler, state.GetHashCode());
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
            _state = null;
        }

        public UniTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return UniTask.CompletedTask;
            }

            _handler?.Invoke(state, message, context);
            return UniTask.CompletedTask;
        }
    }

    internal sealed class StatefulContextualHandlerAction<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Action<TState, PublishingContext> _handler;

        public StatefulContextualHandlerAction(TState state, Action<TState, PublishingContext> handler)
        {
            _state = new(state ?? throw new ArgumentNullException(nameof(state)));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Id = new(handler, state.GetHashCode());
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
            _state = null;
        }

        public UniTask Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return UniTask.CompletedTask;
            }

            _handler?.Invoke(state, context);
            return UniTask.CompletedTask;
        }
    }
}

#endif
