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
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Processing.Internals;
using EncosyTower.Types;
using UnityEngine;

namespace EncosyTower.Processing
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
    using UnityTaskBool = Cysharp.Threading.Tasks.UniTask<bool>;
#else
    using UnityTask = UnityEngine.Awaitable;
    using UnityTaskBool = UnityEngine.Awaitable<bool>;
#endif

    public readonly partial struct ProcessHub<TScope>
    {
        #region    REGISTER - ASYNC
        #endregion ================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ProcessRegistry Register<TRequest>([NotNull] Func<TRequest, UnityTask> process)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest
#endif
        {
            return Register(new Internals.Async.AsyncProcessHandler<TRequest>(process));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ProcessRegistry Register<TRequest, TResult>(
#if UNITASK
            [NotNull] Func<TRequest, Cysharp.Threading.Tasks.UniTask<TResult>> process
#else
            [NotNull] Func<TRequest, UnityEngine.Awaitable<TResult>> process
#endif
        )
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest<TResult>
#endif
        {
            return Register(new Internals.Async.AsyncProcessHandler<TRequest, TResult>(process));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ProcessRegistry Register<TRequest>([NotNull] Func<TRequest, CancellationToken, UnityTask> process)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest
#endif
        {
            return Register(new Internals.Async.CancellableAsyncProcessHandler<TRequest>(process));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ProcessRegistry Register<TRequest, TResult>(
#if UNITASK
            [NotNull] Func<TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>> process
#else
            [NotNull] Func<TRequest, CancellationToken, UnityEngine.Awaitable<TResult>> process
#endif
        )
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest<TResult>
#endif
        {
            return Register(new Internals.Async.CancellableAsyncProcessHandler<TRequest, TResult>(process));
        }

        #region    UNREGISTER - ASYNC
        #endregion ==================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest>(Func<TRequest, UnityTask> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest
#endif
        {
            return Unregister((TypeId)Type<Func<TRequest, UnityTask>>.Id);
        }

#if UNITASK

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest, TResult>(Func<TRequest, Cysharp.Threading.Tasks.UniTask<TResult>> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest<TResult>
#endif
        {
            return Unregister((TypeId)Type<Func<TRequest, Cysharp.Threading.Tasks.UniTask<TResult>>>.Id);
        }

#else // UNITY_6000_0_OR_NEWER

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest, TResult>(Func<TRequest, UnityEngine.Awaitable<TResult>> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest<TResult>
#endif
        {
            return Unregister((TypeId)Type<Func<TRequest, UnityEngine.Awaitable<TResult>>>.Id);
        }

#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest>(Func<TRequest, CancellationToken, UnityTask> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest
#endif
        {
            return Unregister((TypeId)Type<Func<TRequest, CancellationToken, UnityTask>>.Id);
        }

#if UNITASK

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest, TResult>(Func<TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest<TResult>
#endif
        {
            return Unregister((TypeId)Type<Func<TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>>>.Id);
        }

#else // UNITY_6000_0_OR_NEWER

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister<TRequest, TResult>(Func<TRequest, CancellationToken, UnityEngine.Awaitable<TResult>> _)
#if !ENCOSY_PROCESSING_RELAX_MODE
            where TRequest : IAsyncRequest<TResult>
#endif
        {
            return Unregister((TypeId)Type<Func<TRequest, CancellationToken, UnityEngine.Awaitable<TResult>>>.Id);
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityTask ProcessAsync<TRequest>(TRequest request)
        {
            if (TryGet((TypeId)Type<Func<TRequest, UnityTask>>.Id, out var result)
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
                    $"which returns a UniTask " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UnityTaskBool TryProcessAsync<TRequest>(TRequest request, bool silent = false)
        {
            if (TryGet((TypeId)Type<Func<TRequest, UnityTask>>.Id, out var result)
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
                StaticDevLogger.LogError(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"which returns a UniTask " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<TResult>
#else
            UnityEngine.Awaitable<TResult>
#endif
            ProcessAsync<TRequest, TResult>(TRequest request)
        {
#if UNITASK
            var id = (TypeId)Type<Func<TRequest, Cysharp.Threading.Tasks.UniTask<TResult>>>.Id;
#else
            var id = (TypeId)Type<Func<TRequest, UnityEngine.Awaitable<TResult>>>.Id;
#endif

            if (TryGet(id, out var result)
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
                    $"which returns a UniTask<{typeof(TResult)}> " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<TResult>>
#else
            UnityEngine.Awaitable<Option<TResult>>
#endif
            TryProcessAsync<TRequest, TResult>(TRequest request, bool silent = false)
        {
#if UNITASK
            var id = (TypeId)Type<Func<TRequest, Cysharp.Threading.Tasks.UniTask<TResult>>>.Id;
#else
            var id = (TypeId)Type<Func<TRequest, UnityEngine.Awaitable<TResult>>>.Id;
#endif

            if (TryGet(id, out var result)
                && result is IAsyncProcessHandler<TRequest, TResult> handler
            )
            {
                return await handler.ProcessAsync(request, default);
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
                    $"which returns a UniTask<{typeof(TResult)}> " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        public UnityTask ProcessAsync<TRequest>(TRequest request, CancellationToken token)
        {
            if (TryGet((TypeId)Type<Func<TRequest, CancellationToken, UnityTask>>.Id, out var result)
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
                    $"which returns a UniTask " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        public async UnityTaskBool TryProcessAsync<TRequest>(TRequest request, CancellationToken token, bool silent = false)
        {
            if (TryGet((TypeId)Type<Func<TRequest, CancellationToken, UnityTask>>.Id, out var result)
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
                StaticDevLogger.LogError(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"which returns a UniTask " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        public
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<TResult>
#else
            UnityEngine.Awaitable<TResult>
#endif
            ProcessAsync<TRequest, TResult>(TRequest request, CancellationToken token)
        {
#if UNITASK
            var id = (TypeId)Type<Func<TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>>>.Id;
#else
            var id = (TypeId)Type<Func<TRequest, CancellationToken, UnityEngine.Awaitable<TResult>>>.Id;
#endif

            if (TryGet(id, out var result)
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
                    $"which returns a UniTask<{typeof(TResult)}> " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }

        public async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<TResult>>
#else
            UnityEngine.Awaitable<Option<TResult>>
#endif
            TryProcessAsync<TRequest, TResult>(TRequest request, CancellationToken token, bool silent = false)
        {
#if UNITASK
            var id = (TypeId)Type<Func<TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>>>.Id;
#else
            var id = (TypeId)Type<Func<TRequest, CancellationToken, UnityEngine.Awaitable<TResult>>>.Id;
#endif

            if (TryGet(id, out var result)
                && result is IAsyncProcessHandler<TRequest, TResult> handler
            )
            {
                return await handler.ProcessAsync(request, token);
            }

            if (silent == false)
            {
                ErrorNotFound(Scope);
            }

            return Option.None;

            [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
            static void ErrorNotFound(TScope scope)
            {
                StaticDevLogger.LogError(
                    $"Cannot find any process handler for the request {typeof(TRequest)} " +
                    $"which returns a UniTask<{typeof(TResult)}> " +
                    $"inside the scope {typeof(TScope)}({scope})"
                );
            }
        }
    }
}

#endif
