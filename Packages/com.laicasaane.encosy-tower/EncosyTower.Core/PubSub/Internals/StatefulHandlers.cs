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

    internal sealed class StatefulHandlerFuncMessage<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, TMessage, UnityTask> _handler;

        public StatefulHandlerFuncMessage(TState state, Func<TState, TMessage, UnityTask> handler)
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

            return _handler?.Invoke(state, message) ?? UnityTasks.GetCompleted();
        }
    }

    internal sealed class StatefulHandlerFunc<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, UnityTask> _handler;

        public StatefulHandlerFunc(TState state, Func<TState, UnityTask> handler)
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

            return _handler?.Invoke(state) ?? UnityTasks.GetCompleted();
        }
    }

    internal sealed class StatefulHandlerActionMessage<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Action<TState, TMessage> _handler;

        public StatefulHandlerActionMessage(TState state, Action<TState, TMessage> handler)
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

            _handler?.Invoke(state, message);
            return UnityTasks.GetCompleted();
        }
    }

    internal sealed class StatefulHandlerAction<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Action<TState> _handler;

        public StatefulHandlerAction(TState state, Action<TState> handler)
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

            _handler?.Invoke(state);
            return UnityTasks.GetCompleted();
        }
    }
}

#endif
