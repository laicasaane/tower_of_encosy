#if !UNITASK && UNITY_6000_0_OR_NEWER
#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_PROCESSING_NO_VALIDATION__
#else
#define __ENCOSY_PROCESSING_VALIDATION__
#endif

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Modules.Logging;
using EncosyTower.Modules.Processing.Internals;
using UnityEngine;

namespace EncosyTower.Modules.Processing
{
    public readonly partial struct ProcessHub<TScope>
    {
        #region    REGISTER - ASYNC
        #endregion ================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<TypeId> Register<TRequest>([NotNull] Func<TRequest, Awaitable> process)
        {
            return Register(new Internals.Async.AsyncProcessHandler<TRequest>(process));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<TypeId> Register<TRequest, TResult>([NotNull] Func<TRequest, Awaitable<TResult>> process)
        {
            return Register(new Internals.Async.AsyncProcessHandler<TRequest, TResult>(process));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<TypeId> Register<TRequest>([NotNull] Func<TRequest, CancellationToken, Awaitable> process)
        {
            return Register(new Internals.Async.CancellableAsyncProcessHandler<TRequest>(process));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<TypeId> Register<TRequest, TResult>([NotNull] Func<TRequest, CancellationToken, Awaitable<TResult>> process)
        {
            return Register(new Internals.Async.CancellableAsyncProcessHandler<TRequest, TResult>(process));
        }

        #region    UNREGISTER - ASYNC
        #endregion ==================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest>(Func<TRequest, Awaitable> _)
        {
            return Unregister(TypeId.Get<Func<TRequest, Awaitable>>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest>(Func<TRequest, Awaitable<bool>> _)
        {
            return Unregister(TypeId.Get<Func<TRequest, Awaitable<bool>>>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest, TResult>(Func<TRequest, Awaitable<TResult>> _)
        {
            return Unregister(TypeId.Get<Func<TRequest, Awaitable<TResult>>>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest, TResult>(Func<TRequest, Awaitable<Option<TResult>>> _)
        {
            return Unregister(TypeId.Get<Func<TRequest, Awaitable<Option<TResult>>>>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest>(Func<TRequest, CancellationToken, Awaitable> _)
        {
            return Unregister(TypeId.Get<Func<TRequest, CancellationToken, Awaitable>>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest>(Func<TRequest, CancellationToken, Awaitable<bool>> _)
        {
            return Unregister(TypeId.Get<Func<TRequest, CancellationToken, Awaitable<bool>>>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest, TResult>(Func<TRequest, CancellationToken, Awaitable<TResult>> _)
        {
            return Unregister(TypeId.Get<Func<TRequest, CancellationToken, Awaitable<TResult>>>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest, TResult>(Func<TRequest, CancellationToken, Awaitable<Option<TResult>>> _)
        {
            return Unregister(TypeId.Get<Func<TRequest, CancellationToken, Awaitable<Option<TResult>>>>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable ProcessAsync<TRequest>(TRequest request)
        {
            if (TryGet(TypeId.Get<Func<TRequest, Awaitable>>(), out var result)
                && result is IAsyncProcessHandler<TRequest> handler
            )
            {
                return handler.ProcessAsync(request, default);
            }

            throw ExceptionNotFound(Scope);

            static InvalidOperationException ExceptionNotFound(TScope scope)
            {
                return new InvalidOperationException(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"which returns a Awaitable " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Awaitable<bool> TryProcessAsync<TRequest>(TRequest request, bool silent = false)
        {
            if (TryGet(TypeId.Get<Func<TRequest, Awaitable>>(), out var result)
                && result is IAsyncProcessHandler<TRequest> handler
            )
            {
                await handler.ProcessAsync(request, default);
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
                DevLoggerAPI.LogError(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"which returns a Awaitable " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable<TResult> ProcessAsync<TRequest, TResult>(TRequest request)
        {
            if (TryGet(TypeId.Get<Func<TRequest, Awaitable<TResult>>>(), out var result)
                && result is IAsyncProcessHandler<TRequest, TResult> handler
            )
            {
                return handler.ProcessAsync(request, default);
            }

            throw ExceptionNotFound(Scope);

            static InvalidOperationException ExceptionNotFound(TScope scope)
            {
                return new InvalidOperationException(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"which returns a Awaitable<{typeof(TResult)}> " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Awaitable<Option<TResult>> TryProcessAsync<TRequest, TResult>(TRequest request, bool silent = false)
        {
            if (TryGet(TypeId.Get<Func<TRequest, Awaitable<TResult>>>(), out var result)
                && result is IAsyncProcessHandler<TRequest, TResult> handler
            )
            {
                return await handler.ProcessAsync(request, default);
            }

            if (silent == false)
            {
                ErrorNotFound(Scope);
            }

            return default;

            [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
            static void ErrorNotFound(TScope scope)
            {
                DevLoggerAPI.LogError(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"which returns a Awaitable<{typeof(TResult)}> " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        public Awaitable ProcessAsync<TRequest>(TRequest request, CancellationToken token)
        {
            if (TryGet(TypeId.Get<Func<TRequest, CancellationToken, Awaitable>>(), out var result)
                && result is IAsyncProcessHandler<TRequest> handler
            )
            {
                return handler.ProcessAsync(request, token);
            }

            throw ExceptionNotFound(Scope);

            static InvalidOperationException ExceptionNotFound(TScope scope)
            {
                return new InvalidOperationException(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"which returns a Awaitable " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        public async Awaitable<bool> TryProcessAsync<TRequest>(TRequest request, CancellationToken token, bool silent = false)
        {
            if (TryGet(TypeId.Get<Func<TRequest, CancellationToken, Awaitable>>(), out var result)
                && result is IAsyncProcessHandler<TRequest> handler
            )
            {
                await handler.ProcessAsync(request, token);
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
                DevLoggerAPI.LogError(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"which returns a Awaitable " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        public Awaitable<TResult> ProcessAsync<TRequest, TResult>(TRequest request, CancellationToken token)
        {
            if (TryGet(TypeId.Get<Func<TRequest, CancellationToken, Awaitable<TResult>>>(), out var result)
                && result is IAsyncProcessHandler<TRequest, TResult> handler
            )
            {
                return handler.ProcessAsync(request, token);
            }

            throw ExceptionNotFound(Scope);

            static InvalidOperationException ExceptionNotFound(TScope scope)
            {
                return new InvalidOperationException(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"which returns a Awaitable<{typeof(TResult)}> " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        public async Awaitable<Option<TResult>> TryProcessAsync<TRequest, TResult>(TRequest request, CancellationToken token, bool silent = false)
        {
            if (TryGet(TypeId.Get<Func<TRequest, CancellationToken, Awaitable<TResult>>>(), out var result)
                && result is IAsyncProcessHandler<TRequest, TResult> handler
            )
            {
                return await handler.ProcessAsync(request, token);
            }

            if (silent == false)
            {
                ErrorNotFound(Scope);
            }

            return default;

            [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
            static void ErrorNotFound(TScope scope)
            {
                DevLoggerAPI.LogError(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"which returns a Awaitable<{typeof(TResult)}> " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }
    }
}

#endif
