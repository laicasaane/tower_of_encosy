#if !UNITASK && UNITY_6000_0_OR_NEWER
#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __MODULE_CORE_PROCESSING_NO_VALIDATION__
#else
#define __MODULE_CORE_PROCESSING_VALIDATION__
#endif

using System;
using System.Threading;
using UnityEngine;

namespace Module.Core.Processing
{
    public readonly partial struct ProcessHub<TScope, TState>
        where TState : class
    {
        #region    REGISTER - ASYNC
        #endregion ================

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public Option<TypeId> Register<TRequest>(Func<TState, TRequest, Awaitable> process)
        {
            ThrowIfHandlerIsNull(process);

#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Async.AsyncProcessByStateHandler<TState, TRequest>(State, process));
        }

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public Option<TypeId> Register<TRequest, TResult>(Func<TState, TRequest, Awaitable<TResult>> process)
        {
            ThrowIfHandlerIsNull(process);

#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Async.AsyncProcessByStateHandler<TState, TRequest, TResult>(State, process));
        }

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public Option<TypeId> Register<TRequest>(Func<TState, TRequest, CancellationToken, Awaitable> process)
        {
            ThrowIfHandlerIsNull(process);

#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Async.CancellableAsyncProcessByStateHandler<TState, TRequest>(State, process));
        }

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public Option<TypeId> Register<TRequest, TResult>(Func<TState, TRequest, CancellationToken, Awaitable<TResult>> process)
        {
            ThrowIfHandlerIsNull(process);

#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Async.CancellableAsyncProcessByStateHandler<TState, TRequest, TResult>(State, process));
        }

        #region    UNREGISTER - ASYNC
        #endregion ==================

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest>(Func<TState, TRequest, Awaitable> _)
        {
#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, Awaitable>>());
        }

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest>(Func<TState, TRequest, Awaitable<bool>> _)
        {
#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, Awaitable<bool>>>());
        }

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, Awaitable<TResult>> _)
        {
#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, Awaitable<TResult>>>());
        }

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, Awaitable<Option<TResult>>> _)
        {
#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, Awaitable<Option<TResult>>>>());
        }
        
#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest>(Func<TState, TRequest, CancellationToken, Awaitable> _)
        {
#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, CancellationToken, Awaitable>>());
        }

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest>(Func<TState, TRequest, CancellationToken, Awaitable<bool>> _)
        {
#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, CancellationToken, Awaitable<bool>>>());
        }

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, CancellationToken, Awaitable<TResult>> _)
        {
#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, CancellationToken, Awaitable<TResult>>>());
        }

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, CancellationToken, Awaitable<Option<TResult>>> _)
        {
#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, CancellationToken, Awaitable<Option<TResult>>>>());
        }
    }
}

#endif
