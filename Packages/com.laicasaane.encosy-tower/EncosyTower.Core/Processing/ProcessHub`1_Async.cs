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
using EncosyTower.Tasks;
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
            return Register(new Internals.Async.AsyncProcessHandler<TRequest>(process));
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
            return Register(new Internals.Async.AsyncProcessHandler<TRequest, TResult>(process));
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

        #region    PROCESS - ASYNC
        #endregion ===============

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityTask ProcessAsync<TRequest>(TRequest request, bool waitForHandler = false)
        {
            return ProcessAsync(request, CancellationToken.None, waitForHandler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityTaskBool TryProcessAsync<TRequest>(
              TRequest request
            , bool waitForHandler = false
            , bool silent = false
        )
        {
            return TryProcessAsync(request, CancellationToken.None, waitForHandler, silent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<TResult>
#else
            UnityEngine.Awaitable<TResult>
#endif
        ProcessAsync<TRequest, TResult>(TRequest request, bool waitForHandler = false)
        {
            return ProcessAsync<TRequest, TResult>(request, CancellationToken.None, waitForHandler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<TResult>>
#else
            UnityEngine.Awaitable<Option<TResult>>
#endif
        TryProcessAsync<TRequest, TResult>(
              TRequest request
            , bool waitForHandler = false
            , bool silent = false
        )
        {
            return TryProcessAsync<TRequest, TResult>(request, CancellationToken.None, waitForHandler, silent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityTask ProcessAsync<TRequest>(
              TRequest request
            , CancellationToken token
            , bool waitForHandler = false
        )
        {
            if (TryGet(out IAsyncProcessHandler<TRequest> handler, out var hasCandidate))
            {
                return handler.ProcessAsync(request, token);
            }

            if (waitForHandler)
            {
                return WaitThenProcessAsync(this, request, token);
            }

            throw CreateExceptionNotFoundAsync<TRequest>(Scope, hasCandidate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UnityTaskBool TryProcessAsync<TRequest>(
              TRequest request
            , CancellationToken token
            , bool waitForHandler = false
            , bool silent = false
        )
        {
            if (TryGet(out IAsyncProcessHandler<TRequest> handler, out var hasCandidate))
            {
                await handler.ProcessAsync(request, token);
                return true;
            }

            if (waitForHandler)
            {
                await WaitThenProcessAsync(this, request, token);
                return true;
            }

            if (silent == false)
            {
                LogErrorNotFoundAsync<TRequest>(Scope, hasCandidate);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<TResult>
#else
            UnityEngine.Awaitable<TResult>
#endif
        ProcessAsync<TRequest, TResult>(
              TRequest request
            , CancellationToken token
            , bool waitForHandler = false
        )
        {
            if (TryGet(out IAsyncProcessHandler<TRequest, TResult> handler, out var hasCandidate))
            {
                return handler.ProcessAsync(request, token);
            }

            if (waitForHandler)
            {
                return WaitThenProcessAsync<TRequest, TResult>(this, request, token);
            }

            throw CreateExceptionNotFoundAsync<TRequest, TResult>(Scope, hasCandidate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<TResult>>
#else
            UnityEngine.Awaitable<Option<TResult>>
#endif
        TryProcessAsync<TRequest, TResult>(
              TRequest request
            , CancellationToken token
            , bool waitForHandler = false
            , bool silent = false
        )
        {
            if (TryGet(out IAsyncProcessHandler<TRequest, TResult> handler, out var hasCandidate))
            {
                return await handler.ProcessAsync(request, token);
            }

            if (waitForHandler)
            {
                return await WaitThenProcessAsync<TRequest, TResult>(this, request, token);
            }

            if (silent == false)
            {
                LogErrorNotFoundAsync<TRequest, TResult>(Scope, hasCandidate);
            }

            return Option.None;
        }

        #region    CONTAINS HANDLER - ASYNC
        #endregion ========================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsAsyncHandler<TRequest>()
            => TryGet(out IAsyncProcessHandler<TRequest> _, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsAsyncHandler<TRequest, TResult>()
            => TryGet(out IAsyncProcessHandler<TRequest, TResult> _, out _);

        #region    TRY GET - ASYNC
        #endregion ===============

        private bool TryGet<TRequest>(out IAsyncProcessHandler<TRequest> result, out bool hasCandidate)
        {
            var id = (TypeId)Type<Func<TRequest, CancellationToken, UnityTask>>.Id;

            if (TryGet(id, out var candidate))
            {
                hasCandidate = true;

                if (candidate is IAsyncProcessHandler<TRequest> handler)
                {
                    result = handler;
                    return true;
                }
            }
            else
            {
                hasCandidate = false;
            }

            result = default;
            return false;
        }

        private bool TryGet<TRequest, TResult>(
              out IAsyncProcessHandler<TRequest, TResult> result
            , out bool hasCandidate
        )
        {
#if UNITASK
            var id = (TypeId)Type<Func<TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>>>.Id;
#else
            var id = (TypeId)Type<Func<TRequest, CancellationToken, UnityEngine.Awaitable<TResult>>>.Id;
#endif

            if (TryGet(id, out var candidate))
            {
                hasCandidate = true;

                if (candidate is IAsyncProcessHandler<TRequest, TResult> handler)
                {
                    result = handler;
                    return true;
                }
            }
            else
            {
                hasCandidate = false;
            }

            result = default;
            return false;
        }

        #region    HELPERS - SYNC
        #endregion ==============

        private static async UnityTask WaitThenProcessAsync<TRequest>(
              ProcessHub<TScope> hub
            , TRequest request
            , CancellationToken token
        )
        {
            await UnityTasks.WaitUntil(
                  hub
                , static x => x.ContainsAsyncHandler<TRequest>()
                , token
            );

            if (token.IsCancellationRequested)
            {
                return;
            }

            await hub.ProcessAsync(request, token);
        }

        private static async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<TResult>
#else
            UnityEngine.Awaitable<TResult>
#endif
        WaitThenProcessAsync<TRequest, TResult>(
              ProcessHub<TScope> hub
            , TRequest request
            , CancellationToken token
        )
        {
            await UnityTasks.WaitUntil(
                  hub
                , static x => x.ContainsAsyncHandler<TRequest>()
                , token
            );

            if (token.IsCancellationRequested)
            {
                return default;
            }

            return await hub.ProcessAsync<TRequest, TResult>(request, token);
        }

        private static InvalidOperationException CreateExceptionNotFoundAsync<TRequest>(
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
                $"Cannot find any process handler for the request {typeof(TRequest)} which returns " +
#if UNITASK
                $"a UniTask " +
#else
                $"an Awaitable " +
#endif
                $"inside the scope {typeof(TScope)}({scope})"
            );
        }

        private static InvalidOperationException CreateExceptionNotFoundAsync<TRequest, TResult>(
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
                $"Cannot find any process handler for the request {typeof(TRequest)} which returns " +
#if UNITASK
                $"a UniTask<{typeof(TResult)}>" +
#else
                $"an Awaitable<{typeof(TResult)}>" +
#endif
                $"inside the scope {typeof(TScope)}({scope})"
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogErrorNotFoundAsync<TRequest>(TScope scope, bool hasCandidate)
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
                $"Cannot find any process handler for the request {typeof(TRequest)} which returns " +
#if UNITASK
                $"a UniTask " +
#else
                $"an Awaitable " +
#endif
                $"inside the scope {typeof(TScope)}({scope})"
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void LogErrorNotFoundAsync<TRequest, TResult>(TScope scope, bool hasCandidate)
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
                $"Cannot find any process handler for the request {typeof(TRequest)} which returns " +
#if UNITASK
                $"a UniTask<{typeof(TResult)}>" +
#else
                $"an Awaitable<{typeof(TResult)}>" +
#endif
                $"inside the scope {typeof(TScope)}({scope})"
            );
        }
    }
}

#endif
