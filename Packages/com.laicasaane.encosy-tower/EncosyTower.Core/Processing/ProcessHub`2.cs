#if UNITASK || UNITY_6000_0_OR_NEWER
#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_PROCESSING_NO_VALIDATION__
#else
#define __ENCOSY_PROCESSING_VALIDATION__
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Types;

namespace EncosyTower.Processing
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

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ProcessRegistry Register<TRequest>([NotNull] Action<TState, TRequest> process)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IRequest
#endif
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Sync.ProcessByStateHandler<TState, TRequest>(State, process));
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ProcessRegistry Register<TRequest, TResult>([NotNull] Func<TState, TRequest, TResult> process)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IRequest<TResult>
#endif
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Register(new Internals.Sync.ProcessByStateHandler<TState, TRequest, TResult>(State, process));
        }

        #region    UNREGISTER - SYNC
        #endregion =================

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest>(Action<TState, TRequest> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IRequest
#endif
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister((TypeId)Type<Action<TState, TRequest>>.Id);
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister<TRequest, TResult>(Func<TState, TRequest, TResult> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IRequest<TResult>
#endif
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister((TypeId)Type<Func<TState, TRequest, TResult>>.Id);
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister(TypeId id)
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false) return default;
#endif

            return _hub.Unregister(id);
        }

#if __ENCOSY_PROCESSING_VALIDATION__
        private bool Validate()
        {
            if (IsValid)
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
