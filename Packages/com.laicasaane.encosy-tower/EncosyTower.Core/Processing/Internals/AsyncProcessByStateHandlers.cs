#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_PROCESSING_NO_VALIDATION__
#else
#define __ENCOSY_PROCESSING_VALIDATION__
#endif

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Tasks;
using EncosyTower.Types;

namespace EncosyTower.Processing.Internals.Async
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    internal sealed class AsyncProcessByStateHandler<TState, TRequest> : IAsyncProcessHandler<TRequest>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Func<TState, TRequest, UnityTask> _process;

        static AsyncProcessByStateHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, UnityTask>>.Id;
        }

        public AsyncProcessByStateHandler(TState state, Func<TState, TRequest, UnityTask> process)
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
        public UnityTask ProcessAsync(TRequest request, CancellationToken token)
        {
            if (_state.TryGetTarget(out var state))
            {
                return _process(state, request);
            }

#if __ENCOSY_PROCESSING_VALIDATION__
            StateValidation.ErrorIfStateIsDestroyed<TState>();
#endif

            return UnityTasks.GetCompleted();
        }
    }

    internal sealed class AsyncProcessByStateHandler<TState, TRequest, TResult> : IAsyncProcessHandler<TRequest, TResult>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;

#if UNITASK
        private readonly Func<TState, TRequest, Cysharp.Threading.Tasks.UniTask<TResult>> _process;
#else
        private readonly Func<TState, TRequest, UnityEngine.Awaitable<TResult>> _process;
#endif

        static AsyncProcessByStateHandler()
        {
#if UNITASK
            s_typeId = (TypeId)Type<Func<TRequest, Cysharp.Threading.Tasks.UniTask<TResult>>>.Id;
#else
            s_typeId = (TypeId)Type<Func<TRequest, UnityEngine.Awaitable<TResult>>>.Id;
#endif
        }

        public AsyncProcessByStateHandler(
              TState state
#if UNITASK
            , Func<TState, TRequest, Cysharp.Threading.Tasks.UniTask<TResult>> process
#else
            , Func<TState, TRequest, UnityEngine.Awaitable<TResult>> process
#endif
        )
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
        public
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<TResult>
#else
            UnityEngine.Awaitable<TResult>
#endif
            ProcessAsync(TRequest request, CancellationToken token)
        {
            if (_state.TryGetTarget(out var state))
            {
                return _process(state, request);
            }

#if __ENCOSY_PROCESSING_VALIDATION__
            StateValidation.ErrorIfStateIsDestroyed<TState>();
#endif

            return UnityTasks.GetCompleted<TResult>();
        }
    }

    internal sealed class CancellableAsyncProcessByStateHandler<TState, TRequest> : IAsyncProcessHandler<TRequest>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Func<TState, TRequest, CancellationToken, UnityTask> _process;

        static CancellableAsyncProcessByStateHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, CancellationToken, UnityTask>>.Id;
        }

        public CancellableAsyncProcessByStateHandler(TState state, Func<TState, TRequest, CancellationToken, UnityTask> process)
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
        public UnityTask ProcessAsync(TRequest request, CancellationToken token)
        {
            if (_state.TryGetTarget(out var state))
            {
                return _process(state, request, token);
            }

#if __ENCOSY_PROCESSING_VALIDATION__
            StateValidation.ErrorIfStateIsDestroyed<TState>();
#endif

            return UnityTasks.GetCompleted();
        }
    }

    internal sealed class CancellableAsyncProcessByStateHandler<TState, TRequest, TResult> : IAsyncProcessHandler<TRequest, TResult>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;

#if UNITASK
        private readonly Func<TState, TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>> _process;
#else
        private readonly Func<TState, TRequest, CancellationToken, UnityEngine.Awaitable<TResult>> _process;
#endif

        static CancellableAsyncProcessByStateHandler()
        {
#if UNITASK
            s_typeId = (TypeId)Type<Func<TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>>>.Id;
#else
            s_typeId = (TypeId)Type<Func<TRequest, CancellationToken, UnityEngine.Awaitable<TResult>>>.Id;
#endif
        }

        public CancellableAsyncProcessByStateHandler(
              TState state
#if UNITASK
            , Func<TState, TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>> process
#else
            , Func<TState, TRequest, CancellationToken, UnityEngine.Awaitable<TResult>> process
#endif
        )
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
        public
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<TResult>
#else
            UnityEngine.Awaitable<TResult>
#endif
            ProcessAsync(TRequest request, CancellationToken token)
        {
            if (_state.TryGetTarget(out var state))
            {
                return _process(state, request, token);
            }

#if __ENCOSY_PROCESSING_VALIDATION__
            StateValidation.ErrorIfStateIsDestroyed<TState>();
#endif

            return UnityTasks.GetCompleted<TResult>();
        }
    }

#if __ENCOSY_PROCESSING_VALIDATION__
    internal static class StateValidation
    {
        public static void ErrorIfStateIsDestroyed<TState>()
        {
            Logging.StaticLogger.LogError($"The state instance of type {typeof(TState)} is not alive anymore.");
        }
    }
#endif
}

#endif
