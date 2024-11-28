#if UNITASK
#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_PROCESSING_NO_VALIDATION__
#else
#define __ENCOSY_PROCESSING_VALIDATION__
#endif

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace EncosyTower.Modules.Processing.Internals.Async
{
    internal sealed class AsyncProcessByStateHandler<TState, TRequest> : IAsyncProcessHandler<TRequest>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Func<TState, TRequest, UniTask> _process;

        static AsyncProcessByStateHandler()
        {
            s_typeId = (TypeId)TypeId<Func<TRequest, UniTask>>.Value;
        }

        public AsyncProcessByStateHandler(TState state, Func<TState, TRequest, UniTask> process)
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
        public UniTask ProcessAsync(TRequest request, CancellationToken token)
        {
            if (_state.TryGetTarget(out var state))
            {
                return _process(state, request);
            }

#if __ENCOSY_PROCESSING_VALIDATION__
            StateValidation.ErrorIfStateIsDestroyed<TState>();
#endif

            return UniTask.CompletedTask;
        }
    }

    internal sealed class AsyncProcessByStateHandler<TState, TRequest, TResult> : IAsyncProcessHandler<TRequest, TResult>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Func<TState, TRequest, UniTask<TResult>> _process;

        static AsyncProcessByStateHandler()
        {
            s_typeId = (TypeId)TypeId<Func<TRequest, UniTask<TResult>>>.Value;
        }

        public AsyncProcessByStateHandler(TState state, Func<TState, TRequest, UniTask<TResult>> process)
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
        public UniTask<TResult> ProcessAsync(TRequest request, CancellationToken token)
        {
            if (_state.TryGetTarget(out var state))
            {
                return _process(state, request);
            }

#if __ENCOSY_PROCESSING_VALIDATION__
            StateValidation.ErrorIfStateIsDestroyed<TState>();
#endif

            return UniTask.FromResult(default(TResult));
        }
    }

    internal sealed class CancellableAsyncProcessByStateHandler<TState, TRequest> : IAsyncProcessHandler<TRequest>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Func<TState, TRequest, CancellationToken, UniTask> _process;

        static CancellableAsyncProcessByStateHandler()
        {
            s_typeId = (TypeId)TypeId<Func<TRequest, CancellationToken, UniTask>>.Value;
        }

        public CancellableAsyncProcessByStateHandler(TState state, Func<TState, TRequest, CancellationToken, UniTask> process)
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
        public UniTask ProcessAsync(TRequest request, CancellationToken token)
        {
            if (_state.TryGetTarget(out var state))
            {
                return _process(state, request, token);
            }

#if __ENCOSY_PROCESSING_VALIDATION__
            StateValidation.ErrorIfStateIsDestroyed<TState>();
#endif

            return UniTask.CompletedTask;
        }
    }

    internal sealed class CancellableAsyncProcessByStateHandler<TState, TRequest, TResult> : IAsyncProcessHandler<TRequest, TResult>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Func<TState, TRequest, CancellationToken, UniTask<TResult>> _process;

        static CancellableAsyncProcessByStateHandler()
        {
            s_typeId = (TypeId)TypeId<Func<TRequest, CancellationToken, UniTask<TResult>>>.Value;
        }

        public CancellableAsyncProcessByStateHandler(TState state, Func<TState, TRequest, CancellationToken, UniTask<TResult>> process)
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
        public UniTask<TResult> ProcessAsync(TRequest request, CancellationToken token)
        {
            if (_state.TryGetTarget(out var state))
            {
                return _process(state, request, token);
            }

#if __ENCOSY_PROCESSING_VALIDATION__
            StateValidation.ErrorIfStateIsDestroyed<TState>();
#endif

            return UniTask.FromResult(default(TResult));
        }
    }

#if __ENCOSY_PROCESSING_VALIDATION__
    internal static class StateValidation
    {
        public static void ErrorIfStateIsDestroyed<TState>()
        {
            Logging.RuntimeLoggerAPI.LogError($"The state instance of type {typeof(TState)} is not alive anymore.");
        }
    }
#endif
}

#endif
