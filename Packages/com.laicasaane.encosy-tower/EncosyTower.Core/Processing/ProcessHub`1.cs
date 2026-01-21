#if UNITASK || UNITY_6000_0_OR_NEWER
#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_PROCESSING_NO_VALIDATION__
#else
#define __ENCOSY_PROCESSING_VALIDATION__
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Processing.Internals;
using EncosyTower.Types;
using UnityEngine;

namespace EncosyTower.Processing
{
    public static partial class ProcessHubExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProcessHub<TScope> WithRegistries<TScope>(
              this in ProcessHub<TScope> hub
            , ICollection<ProcessRegistry> registries
        )
        {
            return new ProcessHub<TScope>(hub.Scope, hub._map, registries ?? EmptyRegistries.Default);
        }
    }

    public readonly partial struct ProcessHub<TScope> : IIsCreated
    {
        internal readonly ProcessHandlerMap _map;
        internal readonly ICollection<ProcessRegistry> _registries;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ProcessHub(TScope scope, [NotNull] ProcessHandlerMap map)
        {
            _map = map;
            _registries = EmptyRegistries.Default;
            Scope = scope;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ProcessHub(
              TScope scope
            , [NotNull] ProcessHandlerMap map
            , [NotNull] ICollection<ProcessRegistry> registries
        )
        {
            _map = map;
            _registries = registries;
            Scope = scope;
        }

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map != null;
        }

        public TScope Scope { get; }

        public ICollection<ProcessRegistry> Registries => _registries ?? EmptyRegistries.Default;

        #region    REGISTER - SYNC
        #endregion ===============

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ProcessRegistry Register<TRequest>([NotNull] Action<TRequest> process)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IRequest
#endif
        {
            return Register(new Internals.Sync.ProcessHandler<TRequest>(process));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ProcessRegistry Register<TRequest, TResult>([NotNull] Func<TRequest, TResult> process)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IRequest<TResult>
#endif
        {
            return Register(new Internals.Sync.ProcessHandler<TRequest, TResult>(process));
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal ProcessRegistry Register(IProcessHandler handler)
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false)
            {
                return default;
            }
#endif

            var registry = _map.Register(handler);
            Registries.Add(registry);

            return registry;
        }

        #region    UNREGISTER - SYNC
        #endregion =================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest>(Action<TRequest> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IRequest
#endif
        {
            return Unregister((TypeId)Type<Action<TRequest>>.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest, TResult>(Func<TRequest, TResult> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IRequest<TResult>
#endif
        {
            return Unregister((TypeId)Type<Func<TRequest, TResult>>.Id);
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Unregister(TypeId id)
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false)
            {
                return false;
            }
#endif

            return _map.Unregister(id);
        }

        #region    PROCESS - SYNC
        #endregion ==============

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Process<TRequest>(TRequest request)
        {
            if (TryGet(out IProcessHandler<TRequest> handler, out var hasCandidate))
            {
                handler.Process(request);
                return;
            }

            throw CreateExceptionNotFound<TRequest>(Scope, hasCandidate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryProcess<TRequest>(TRequest request, bool silent = false)
        {
            if (TryGet(out IProcessHandler<TRequest> handler, out var hasCandidate))
            {
                handler.Process(request);
                return true;
            }

            if (silent == false)
            {
                LogErrorNotFound<TRequest>(Scope, hasCandidate);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Process<TRequest, TResult>(TRequest request)
        {
            if (TryGet(out IProcessHandler<TRequest, TResult> handler, out var hasCandidate))
            {
                return handler.Process(request);
            }

            throw CreateExceptionNotFound<TRequest, TResult>(Scope, hasCandidate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<TResult> TryProcess<TRequest, TResult>(TRequest request, bool silent = false)
        {
            if (TryGet(out IProcessHandler<TRequest, TResult> handler, out var hasCandidate))
            {
                return handler.Process(request);
            }

            if (silent == false)
            {
                LogErrorNotFound<TRequest, TResult>(Scope, hasCandidate);
            }

            return Option.None;
        }

        #region    CONTAINS HANDLER - ASYNC
        #endregion ========================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsHandler<TRequest>()
            => TryGet(out IProcessHandler<TRequest> _, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsHandler<TRequest, TResult>()
            => TryGet(out IProcessHandler<TRequest, TResult> _, out _);

        #region    TRY GET - SYNC
        #endregion ==============

        private bool TryGet<TRequest>(out IProcessHandler<TRequest> result, out bool hasCandidate)
        {
            var id = (TypeId)Type<Action<TRequest>>.Id;

            if (TryGet(id, out var candidate))
            {
                hasCandidate = true;

                if (candidate is IProcessHandler<TRequest> handler)
                {
                    result = handler;
                    return true;
                }
            }
            else
            {
                hasCandidate = false;
            }

            result = null;
            return false;
        }

        private bool TryGet<TRequest, TResult>(out IProcessHandler<TRequest, TResult> result, out bool hasCandidate)
        {
            var id = (TypeId)Type<Func<TRequest, TResult>>.Id;

            if (TryGet(id, out var candidate))
            {
                hasCandidate = true;

                if (candidate is IProcessHandler<TRequest, TResult> handler)
                {
                    result = handler;
                    return true;
                }
            }
            else
            {
                hasCandidate = false;
            }

            result = null;
            return false;
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool TryGet(TypeId typeId, out IProcessHandler handler)
        {
#if __ENCOSY_PROCESSING_VALIDATION__
            if (Validate() == false)
            {
                handler = null;
                return false;
            }
#endif

            return _map.TryGet(typeId, out handler);
        }

        #region    HELPERS - SYNC
        #endregion ==============

#if __ENCOSY_PROCESSING_VALIDATION__
        private bool Validate()
        {
            if (_map != null)
            {
                return true;
            }

            StaticLogger.LogError(
                $"{GetType().Name} must be retrieved via `{nameof(Processor)}.{nameof(Scope)}` API"
            );

            return false;
        }
#endif

        private static InvalidOperationException CreateExceptionNotFound<TRequest>(TScope scope, bool hasCandidate)
        {
            if (hasCandidate)
            {
                return new InvalidOperationException(
                    $"Found a candidate process handler for the request {typeof(TRequest)} " +
                    $"inside the scope {typeof(TScope)}({scope}), " +
                    $"but it has an invalid type."
                );
            }

            return new InvalidOperationException(
                $"Cannot find any process handler for the request {typeof(TRequest)} " +
                $"inside the scope {typeof(TScope)}({scope})"
            );
        }

        private static InvalidOperationException CreateExceptionNotFound<TRequest, TResult>(
              TScope scope
            , bool hasCandidate
        )
        {
            if (hasCandidate)
            {
                return new InvalidOperationException(
                    $"Found a candidate process handler for the request {typeof(TRequest)} " +
                    $"inside the scope {typeof(TScope)}({scope}), " +
                    $"but it has an invalid type."
                );
            }

            return new InvalidOperationException(
                $"Cannot find any process handler for the request {typeof(TRequest)} " +
                $"which returns a {typeof(TResult)} " +
                $"inside the scope {typeof(TScope)}({scope})"
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogErrorNotFound<TRequest>(TScope scope, bool hasCandidate)
        {
            if (hasCandidate)
            {
                StaticDevLogger.LogError(
                    $"Found a candidate process handler for the request {typeof(TRequest)} " +
                    $"inside the scope {typeof(TScope)}({scope}), " +
                    $"but it has an invalid type."
                );
                return;
            }

            StaticDevLogger.LogError(
                $"Cannot find any process handler for the request {typeof(TRequest)} " +
                $"inside the scope {typeof(TScope)}({scope})"
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogErrorNotFound<TRequest, TResult>(TScope scope, bool hasCandidate)
        {
            if (hasCandidate)
            {
                StaticDevLogger.LogError(
                    $"Found a candidate process handler for the request {typeof(TRequest)} " +
                    $"inside the scope {typeof(TScope)}({scope}), " +
                    $"but it has an invalid type."
                );
                return;
            }

            StaticDevLogger.LogError(
                $"Cannot find any process handler for the request {typeof(TRequest)} " +
                $"which returns a {typeof(TResult)} " +
                $"inside the scope {typeof(TScope)}({scope})"
            );
        }
    }
}

#endif
