#if !UNITASK && UNITY_6000_0_OR_NEWER
#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_PROCESSING_NO_VALIDATION__
#else
#define __ENCOSY_PROCESSING_VALIDATION__
#endif

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Modules.Processing.Internals.Async
{
    internal sealed class AsyncProcessByStateHandler<TState, TRequest> : IAsyncProcessHandler<TRequest>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Func<TState, TRequest, Awaitable> _process;

        static AsyncProcessByStateHandler()
        {
            s_typeId = TypeId.Get<Func<TRequest, Awaitable>>();
        }

        public AsyncProcessByStateHandler(TState state, Func<TState, TRequest, Awaitable> process)
        {
            _state = new(state ?? throw new ArgumentNullException(nameof(state)));
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable ProcessAsync(TRequest request, CancellationToken token)
        {
            if (_state.TryGetTarget(out var state))
            {
                return _process(state, request);
            }

#if __ENCOSY_PROCESSING_VALIDATION__
            StateValidation.ErrorIfStateIsDestroyed<TState>();
#endif

            return Awaitables.GetCompleted();
        }
    }

    internal sealed class AsyncProcessByStateHandler<TState, TRequest, TResult> : IAsyncProcessHandler<TRequest, TResult>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Func<TState, TRequest, Awaitable<TResult>> _process;

        static AsyncProcessByStateHandler()
        {
            s_typeId = TypeId.Get<Func<TRequest, Awaitable<TResult>>>();
        }

        public AsyncProcessByStateHandler(TState state, Func<TState, TRequest, Awaitable<TResult>> process)
        {
            _state = new(state ?? throw new ArgumentNullException(nameof(state)));
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable<TResult> ProcessAsync(TRequest request, CancellationToken token)
        {
            if (_state.TryGetTarget(out var state))
            {
                return _process(state, request);
            }

#if __ENCOSY_PROCESSING_VALIDATION__
            StateValidation.ErrorIfStateIsDestroyed<TState>();
#endif

            return Awaitables.GetCompleted<TResult>();
        }
    }

    internal sealed class CancellableAsyncProcessByStateHandler<TState, TRequest> : IAsyncProcessHandler<TRequest>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Func<TState, TRequest, CancellationToken, Awaitable> _process;

        static CancellableAsyncProcessByStateHandler()
        {
            s_typeId = TypeId.Get<Func<TRequest, CancellationToken, Awaitable>>();
        }

        public CancellableAsyncProcessByStateHandler(TState state, Func<TState, TRequest, CancellationToken, Awaitable> process)
        {
            _state = new(state ?? throw new ArgumentNullException(nameof(state)));
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable ProcessAsync(TRequest request, CancellationToken token)
        {
            if (_state.TryGetTarget(out var state))
            {
                return _process(state, request, token);
            }

#if __ENCOSY_PROCESSING_VALIDATION__
            StateValidation.ErrorIfStateIsDestroyed<TState>();
#endif

            return Awaitables.GetCompleted();
        }
    }

    internal sealed class CancellableAsyncProcessByStateHandler<TState, TRequest, TResult> : IAsyncProcessHandler<TRequest, TResult>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Func<TState, TRequest, CancellationToken, Awaitable<TResult>> _process;

        static CancellableAsyncProcessByStateHandler()
        {
            s_typeId = TypeId.Get<Func<TRequest, CancellationToken, Awaitable<TResult>>>();
        }

        public CancellableAsyncProcessByStateHandler(TState state, Func<TState, TRequest, CancellationToken, Awaitable<TResult>> process)
        {
            _state = new(state ?? throw new ArgumentNullException(nameof(state)));
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable<TResult> ProcessAsync(TRequest request, CancellationToken token)
        {
            if (_state.TryGetTarget(out var state))
            {
                return _process(state, request, token);
            }

#if __ENCOSY_PROCESSING_VALIDATION__
            StateValidation.ErrorIfStateIsDestroyed<TState>();
#endif

            return Awaitables.GetCompleted<TResult>();
        }
    }

#if __ENCOSY_PROCESSING_VALIDATION__
    internal static class StateValidation
    {
        public static void ErrorIfStateIsDestroyed<TState>()
        {
            Logging.RuntimeLoggerAPI.LogError($"The state instance of type {typeof(TState)} is not alive anymore." );
        }
    }
#endif
}

#endif
