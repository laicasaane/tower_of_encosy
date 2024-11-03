#if UNITASK || UNITY_6000_0_OR_NEWER
#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __MODULE_CORE_PROCESSING_NO_VALIDATION__
#else
#define __MODULE_CORE_PROCESSING_VALIDATION__
#endif

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Processing
{
    public static partial class ProcessHubExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProcessHub<TScope, TState> WithState<TScope, TState>(
              this ProcessHub<TScope> hub
            , [NotNull] TState state
        )
            where TState : class
        {
            return new ProcessHub<TScope, TState>(hub, state);
        }
    }

    public readonly partial struct ProcessHub<TScope, TState>
        where TState : class
    {
        private readonly ProcessHub<TScope> _hub;

        public TScope Scope => _hub.Scope;

        public bool IsValid => _hub.IsValid;

        public TState State { get; }

        internal ProcessHub(ProcessHub<TScope> hub, [NotNull] TState state)
        {
            _hub = hub;
            State = state;
        }

        #region    REGISTER - SYNC
        #endregion ===============

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public Option<TypeId> Register<TRequest>(Action<TState, TRequest> process)
        {
            ThrowIfHandlerIsNull(process);

#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Sync.ProcessByStateHandler<TState, TRequest>(State, process));
        }

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public Option<TypeId> Register<TRequest, TResult>(Func<TState, TRequest, TResult> process)
        {
            ThrowIfHandlerIsNull(process);

#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Sync.ProcessByStateHandler<TState, TRequest, TResult>(State, process));
        }

        #region    UNREGISTER - SYNC
        #endregion =================

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest>(Action<TState, TRequest> _)
        {
#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Action<TState, TRequest>>());
        }

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest>(Func<TState, TRequest, bool> _)
        {
#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, bool>>());
        }

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, TResult> _)
        {
#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, TResult>>());
        }

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, Option<TResult>> _)
        {
#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(TypeId.Get<Func<TState, TRequest, Option<TResult>>>());
        }

#if __MODULE_CORE_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister(TypeId id)
        {
#if __MODULE_CORE_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(id);
        }

        [Conditional("__MODULE_CORE_PROCESSING_VALIDATION__"), DoesNotReturn]
        private static void ThrowIfHandlerIsNull(Delegate process)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));
        }

#if __MODULE_CORE_PROCESSING_VALIDATION__
        private bool Validate()
        {
            if (IsValid == true)
            {
                return true;
            }

            Logging.RuntimeLoggerAPI.LogError(
                $"{GetType().Name} must be retrieved via `{nameof(Processor)}.{Scope}` API"
            );

            return false;
        }
#endif
    }
}

#endif
