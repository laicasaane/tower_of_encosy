#if UNITASK
#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __ENCOSY_PROCESSING_NO_VALIDATION__
#else
#define __ENCOSY_PROCESSING_VALIDATION__
#endif

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace EncosyTower.Modules.Processing
{
    public readonly partial struct ProcessHub<TScope, TState>
        where TState : class
    {
        #region    REGISTER - ASYNC
        #endregion ================

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public Option<TypeId> Register<TRequest>(Func<TState, TRequest, UniTask> process)
        {
            ThrowIfHandlerIsNull(process);

#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Async.AsyncProcessByStateHandler<TState, TRequest>(State, process));
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public Option<TypeId> Register<TRequest, TResult>(Func<TState, TRequest, UniTask<TResult>> process)
        {
            ThrowIfHandlerIsNull(process);

#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Async.AsyncProcessByStateHandler<TState, TRequest, TResult>(State, process));
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public Option<TypeId> Register<TRequest>(Func<TState, TRequest, CancellationToken, UniTask> process)
        {
            ThrowIfHandlerIsNull(process);

#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Async.CancellableAsyncProcessByStateHandler<TState, TRequest>(State, process));
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public Option<TypeId> Register<TRequest, TResult>(Func<TState, TRequest, CancellationToken, UniTask<TResult>> process)
        {
            ThrowIfHandlerIsNull(process);

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
        public bool Unregister<TRequest>(Func<TState, TRequest, UniTask> _)
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, UniTask>>());
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest>(Func<TState, TRequest, UniTask<bool>> _)
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, UniTask<bool>>>());
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, UniTask<TResult>> _)
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, UniTask<TResult>>>());
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, UniTask<Option<TResult>>> _)
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, UniTask<Option<TResult>>>>());
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest>(Func<TState, TRequest, CancellationToken, UniTask> _)
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, CancellationToken, UniTask>>());
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest>(Func<TState, TRequest, CancellationToken, UniTask<bool>> _)
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, CancellationToken, UniTask<bool>>>());
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, CancellationToken, UniTask<TResult>> _)
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, CancellationToken, UniTask<TResult>>>());
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, CancellationToken, UniTask<Option<TResult>>> _)
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, CancellationToken, UniTask<Option<TResult>>>>());
        }
    }
}

#endif
