#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Types;

namespace EncosyTower.Processing.Internals.Async
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    internal sealed class AsyncProcessHandler<TRequest> : IAsyncProcessHandler<TRequest>
    {
        private static readonly TypeId s_typeId;
        private readonly Func<TRequest, UnityTask> _process1;
        private readonly Func<TRequest, CancellationToken, UnityTask> _process2;

        static AsyncProcessHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, CancellationToken, UnityTask>>.Id;
        }

        public AsyncProcessHandler(Func<TRequest, UnityTask> process)
        {
            _process1 = process ?? throw new ArgumentNullException(nameof(process));
        }

        public AsyncProcessHandler(Func<TRequest, CancellationToken, UnityTask> process)
        {
            _process2 = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityTask ProcessAsync(TRequest request, CancellationToken token)
        {
            return _process1 is not null
                ? _process1(request)
                : _process2(request, token);
        }
    }

    internal sealed class AsyncProcessHandler<TRequest, TResult> : IAsyncProcessHandler<TRequest, TResult>
    {
        private static readonly TypeId s_typeId;

#if UNITASK
        private readonly Func<TRequest, Cysharp.Threading.Tasks.UniTask<TResult>> _process1;
        private readonly Func<TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>> _process2;
#else
        private readonly Func<TRequest, UnityEngine.Awaitable<TResult>> _process1;
        private readonly Func<TRequest, CancellationToken, UnityEngine.Awaitable<TResult>> _process2;
#endif

        static AsyncProcessHandler()
        {
#if UNITASK
            s_typeId = (TypeId)Type<Func<TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>>>.Id;
#else
            s_typeId = (TypeId)Type<Func<TRequest, CancellationToken, UnityEngine.Awaitable<TResult>>>.Id;
#endif
        }

        public AsyncProcessHandler(
#if UNITASK
            Func<TRequest, Cysharp.Threading.Tasks.UniTask<TResult>> process
#else
            Func<TRequest, UnityEngine.Awaitable<TResult>> process
#endif
        )
        {
            _process1 = process ?? throw new ArgumentNullException(nameof(process));
        }

        public AsyncProcessHandler(
#if UNITASK
            Func<TRequest, CancellationToken, Cysharp.Threading.Tasks.UniTask<TResult>> process
#else
            Func<TRequest, CancellationToken, UnityEngine.Awaitable<TResult>> process
#endif
        )
        {
            _process2 = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<TResult>
#else
            UnityEngine.Awaitable<TResult>
#endif
            ProcessAsync(TRequest request, CancellationToken token)
        {
            return _process1 is not null
                ? _process1(request)
                : _process2(request, token);
        }
    }
}

#endif
