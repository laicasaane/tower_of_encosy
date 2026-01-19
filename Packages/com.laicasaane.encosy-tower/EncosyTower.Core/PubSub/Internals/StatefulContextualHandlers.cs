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

    internal sealed class StatefulContextualHandlerFuncMessage<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, TMessage, PublishingContext, UnityTask> _handler;

        public StatefulContextualHandlerFuncMessage(TState state, Func<TState, TMessage, PublishingContext, UnityTask> handler)
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

        public UnityTask Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return UnityTasks.GetCompleted();
            }

            return _handler?.Invoke(state, message, context) ?? UnityTasks.GetCompleted();
        }
    }

    internal sealed class StatefulContextualHandlerFunc<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, PublishingContext, UnityTask> _handler;

        public StatefulContextualHandlerFunc(TState state, Func<TState, PublishingContext, UnityTask> handler)
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

        public UnityTask Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return UnityTasks.GetCompleted();
            }

            return _handler?.Invoke(state, context) ?? UnityTasks.GetCompleted();
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

        public UnityTask Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return UnityTasks.GetCompleted();
            }

            _handler?.Invoke(state, message, context);
            return UnityTasks.GetCompleted();
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

        public UnityTask Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested || _state.TryGetTarget(out var state) == false)
            {
                return UnityTasks.GetCompleted();
            }

            _handler?.Invoke(state, context);
            return UnityTasks.GetCompleted();
        }
    }
}

#endif
