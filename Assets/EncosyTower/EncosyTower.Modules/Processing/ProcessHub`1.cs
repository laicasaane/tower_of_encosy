#if UNITASK || UNITY_6000_0_OR_NEWER
#if !(UNITY_EDITOR || DEBUG) || DISABLE_DEBUG
#define __ENCOSY_PROCESSING_NO_VALIDATION__
#else
#define __ENCOSY_PROCESSING_VALIDATION__
#endif

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Modules.Logging;
using EncosyTower.Modules.Processing.Internals;
using UnityEngine;

namespace EncosyTower.Modules.Processing
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
        public Option<TypeId> Register<TRequest>(Action<TRequest> process)
        {
            ThrowIfHandlerIsNull(process);
            return Register(new Internals.Sync.ProcessHandler<TRequest>(process));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<TypeId> Register<TRequest, TResult>(Func<TRequest, TResult> process)
        {
            ThrowIfHandlerIsNull(process);
            return Register(new Internals.Sync.ProcessHandler<TRequest, TResult>(process));
        }

#if __ENCOSY_PROCESSING_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal Option<TypeId> Register(IProcessHandler handler)
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
        {
            return Unregister(TypeId.Get<Action<TRequest>>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest>(Func<TRequest, bool> _)
        {
            return Unregister(TypeId.Get<Func<TRequest, bool>>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest, TResult>(Func<TRequest, TResult> _)
        {
            return Unregister(TypeId.Get<Func<TRequest, TResult>>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest, TResult>(Func<TRequest, Option<TResult>> _)
        {
            return Unregister(TypeId.Get<Func<TRequest, Option<TResult>>>());
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
            if (TryGet(TypeId.Get<Action<TRequest>>(), out var result)
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
            if (TryGet(TypeId.Get<Action<TRequest>>(), out var result)
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

            [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
            static void ErrorNotFound(TScope scope)
            {
                DevLoggerAPI.LogError(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        public TResult Process<TRequest, TResult>(TRequest request)
        {
            if (TryGet(TypeId.Get<Func<TRequest, TResult>>(), out var result)
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
            if (TryGet(TypeId.Get<Func<TRequest, TRequest>>(), out var result)
                && result is IProcessHandler<TRequest, TResult> handler
            )
            {
                return handler.Process(request);
            }

            if (silent == false)
            {
                ErrorNotFound(Scope);
            }

            return default;

            [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
            static void ErrorNotFound(TScope scope)
            {
                DevLoggerAPI.LogError(
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

        [HideInCallstack, DoesNotReturn, Conditional("__ENCOSY_PROCESSING_VALIDATION__")]
        private static void ThrowIfHandlerIsNull(Delegate @delegate)
        {
            if (@delegate == null) throw new ArgumentNullException(nameof(@delegate));
        }

#if __ENCOSY_PROCESSING_VALIDATION__
        private bool Validate()
        {
            if (_map != null)
            {
                return true;
            }

            RuntimeLoggerAPI.LogError(
                $"{GetType().Name} must be retrieved via `{nameof(Processor)}.{nameof(Scope)}` API"
            );

            return false;
        }
#endif
    }
}

#endif
