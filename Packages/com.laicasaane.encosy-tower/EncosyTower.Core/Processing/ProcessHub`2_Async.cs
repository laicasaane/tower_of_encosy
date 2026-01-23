#if UNITASK || UNITY_6000_0_OR_NEWER

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_PROCESSING_NO_VALIDATION__
#else
#define __ENCOSY_PROCESSING_VALIDATION__
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Types;

namespace EncosyTower.Processing
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public readonly partial struct ProcessHub<TScope, TState>
        where TState : class
    {
        #region    REGISTER - ASYNC
        #endregion ================

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ProcessRegistry Register<TRequest>([NotNull] Func<TState, TRequest, UnityTask> process)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest
#endif
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Async.AsyncProcessByStateHandler<TState, TRequest>(State, process));
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ProcessRegistry Register<TRequest, TResult>(
#if UNITASK
            [NotNull] Func<TState, TRequest, Cysharp.Threading.Tasks.UniTask<TResult>> process
#else
            [NotNull] Func<TState, TRequest, UnityEngine.Awaitable<TResult>> process
#endif
        )
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest<TResult>
#endif
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Async.AsyncProcessByStateHandler<TState, TRequest, TResult>(State, process));
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ProcessRegistry Register<TRequest>([NotNull] Func<TState, TRequest, CancellationToken, UnityTask> process)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest
#endif
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Async.CancellableAsyncProcessByStateHandler<TState, TRequest>(State, process));
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ProcessRegistry Register<TRequest, TResult>(
#if UNITASK
            [NotNull] Func<TState, TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>> process
#else
            [NotNull] Func<TState, TRequest, CancellationToken, UnityEngine.Awaitable<TResult>> process
#endif
        )
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest<TResult>
#endif
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Async.CancellableAsyncProcessByStateHandler<TState, TRequest, TResult>(State, process));
        }

        #region    UNREGISTER - ASYNC
        #endregion ==================

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest>(Func<TState, TRequest, UnityTask> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest
#endif
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister((TypeId)Type<Func<TState, TRequest, UnityTask>>.Id);
        }

#if UNITASK

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, Cysharp.Threading.Tasks.UniTask<TResult>> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest<TResult>
#endif
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister((TypeId)Type<Func<TState, TRequest, Cysharp.Threading.Tasks.UniTask<TResult>>>.Id);
        }

#else // UNITY_6000_0_OR_NEWER

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, UnityEngine.Awaitable<TResult>> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest<TResult>
#endif
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister((TypeId)Type<Func<TState, TRequest, UnityEngine.Awaitable<TResult>>>.Id);
        }

#endif

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest>(Func<TState, TRequest, CancellationToken, UnityTask> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest
#endif
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister((TypeId)Type<Func<TState, TRequest, CancellationToken, UnityTask>>.Id);
        }

#if UNITASK

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest<TResult>
#endif
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister((TypeId)Type<Func<TState, TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>>>.Id);
        }

#else // UNITY_6000_0_OR_NEWER

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, CancellationToken, UnityEngine.Awaitable<TResult>> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest<TResult>
#endif
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister((TypeId)Type<Func<TState, TRequest, CancellationToken, UnityEngine.Awaitable<TResult>>>.Id);
        }

#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        partial void RetainUsings_Async();
    }
}

#endif
