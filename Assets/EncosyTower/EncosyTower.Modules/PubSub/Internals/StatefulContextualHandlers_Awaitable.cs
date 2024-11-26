#if !UNITASK && UNITY_6000_0_OR_NEWER

using System;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Modules.PubSub.Internals
{
    internal sealed class StatefulContextualHandlerFuncMessageToken<TState, TMessage> : IHandler<TMessage>
        where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, TMessage, PublishingContext, CancellationToken, Awaitable> _handler;

        public StatefulContextualHandlerFuncMessageToken(TState state, Func<TState, TMessage, PublishingContext, CancellationToken, Awaitable> handler)
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

        public Awaitable Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return Awaitables.GetCompleted();
            }

            return _handler?.Invoke(state, message, context, token) ?? Awaitables.GetCompleted();
        }
    }

    internal sealed class StatefulContextualHandlerFuncToken<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, PublishingContext, CancellationToken, Awaitable> _handler;

        public StatefulContextualHandlerFuncToken(TState state, Func<TState, PublishingContext, CancellationToken, Awaitable> handler)
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

        public Awaitable Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return Awaitables.GetCompleted();
            }

            return _handler?.Invoke(state, context, token) ?? Awaitables.GetCompleted();
        }
    }

    internal sealed class StatefulContextualHandlerFuncMessage<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, TMessage, PublishingContext, Awaitable> _handler;

        public StatefulContextualHandlerFuncMessage(TState state, Func<TState, TMessage, PublishingContext, Awaitable> handler)
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

        public Awaitable Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return Awaitables.GetCompleted();
            }

            return _handler?.Invoke(state, message, context) ?? Awaitables.GetCompleted();
        }
    }

    internal sealed class StatefulContextualHandlerFunc<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, PublishingContext, Awaitable> _handler;

        public StatefulContextualHandlerFunc(TState state, Func<TState, PublishingContext, Awaitable> handler)
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

        public Awaitable Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return Awaitables.GetCompleted();
            }

            return _handler?.Invoke(state, context) ?? Awaitables.GetCompleted();
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

        public Awaitable Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return Awaitables.GetCompleted();
            }

            _handler?.Invoke(state, message, context);
            return Awaitables.GetCompleted();
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

        public Awaitable Handle(TMessage message, PublishingContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return Awaitables.GetCompleted();
            }

            _handler?.Invoke(state, context);
            return Awaitables.GetCompleted();
        }
    }
}

#endif
