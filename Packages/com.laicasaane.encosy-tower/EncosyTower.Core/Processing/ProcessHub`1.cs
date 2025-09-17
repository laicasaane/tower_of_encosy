#if UNITASK || UNITY_6000_0_OR_NEWER
#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_PROCESSING_NO_VALIDATION__
#else
#define __ENCOSY_PROCESSING_VALIDATION__
#endif

using System;
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
    public readonly partial struct ProcessHub<TScope>
    {
        private readonly ProcessHandlerMap _map;

        public readonly TScope Scope;

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map != null;
        }

        internal ProcessHub(TScope scope, ProcessHandlerMap map)
        {
            _map = map;
            Scope = scope;
        }

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

            return _map.Register(handler);
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

        public void Process<TRequest>(TRequest request)
        {
            if (TryGet((TypeId)Type<Action<TRequest>>.Id, out var result)
                && result is IProcessHandler<TRequest> handler
            )
            {
                handler.Process(request);
                return;
            }

            throw ExceptionNotFound(Scope);

            static InvalidOperationException ExceptionNotFound(TScope scope)
            {
                return new InvalidOperationException(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        public bool TryProcess<TRequest>(TRequest request, bool silent = false)
        {
            if (TryGet((TypeId)Type<Action<TRequest>>.Id, out var result)
                && result is IProcessHandler<TRequest> handler
            )
            {
                handler.Process(request);
                return true;
            }

            if (silent == false)
            {
                ErrorNotFound(Scope);
            }

            return false;

            [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
            static void ErrorNotFound(TScope scope)
            {
                StaticDevLogger.LogError(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        public TResult Process<TRequest, TResult>(TRequest request)
        {
            if (TryGet((TypeId)Type<Func<TRequest, TResult>>.Id, out var result)
                && result is IProcessHandler<TRequest, TResult> handler
            )
            {
                return handler.Process(request);
            }

            throw ExceptionNotFound(Scope);

            static InvalidOperationException ExceptionNotFound(TScope scope)
            {
                return new InvalidOperationException(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"which returns a {typeof(TResult)} " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        public Option<TResult> TryProcess<TRequest, TResult>(TRequest request, bool silent = false)
        {
            if (TryGet((TypeId)Type<Func<TRequest, TResult>>.Id, out var result)
                && result is IProcessHandler<TRequest, TResult> handler
            )
            {
                return handler.Process(request);
            }

            if (silent == false)
            {
                ErrorNotFound(Scope);
            }

            return Option.None;

            [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
            static void ErrorNotFound(TScope scope)
            {
                StaticDevLogger.LogError(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"which returns a {typeof(TResult)} " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
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
    }
}

#endif
