#if UNITASK

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace EncosyTower.Modules.Processing.Internals.Async
{
    internal sealed class AsyncProcessHandler<TRequest> : IAsyncProcessHandler<TRequest>
    {
        private static readonly TypeId s_typeId;
        private readonly Func<TRequest, UniTask> _process;

        static AsyncProcessHandler()
        {
            s_typeId = TypeId.Get<Func<TRequest, UniTask>>();
        }

        public AsyncProcessHandler(Func<TRequest, UniTask> process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask ProcessAsync(TRequest request, CancellationToken token)
        {
            return _process(request);
        }
    }

    internal sealed class AsyncProcessHandler<TRequest, TResult> : IAsyncProcessHandler<TRequest, TResult>
    {
        private static readonly TypeId s_typeId;
        private readonly Func<TRequest, UniTask<TResult>> _process;

        static AsyncProcessHandler()
        {
            s_typeId = TypeId.Get<Func<TRequest, UniTask<TResult>>>();
        }

        public AsyncProcessHandler(Func<TRequest, UniTask<TResult>> process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TResult> ProcessAsync(TRequest request, CancellationToken token)
        {
            return _process(request);
        }
    }

    internal sealed class CancellableAsyncProcessHandler<TRequest> : IAsyncProcessHandler<TRequest>
    {
        private static readonly TypeId s_typeId;
        private readonly Func<TRequest, CancellationToken, UniTask> _process;

        static CancellableAsyncProcessHandler()
        {
            s_typeId = TypeId.Get<Func<TRequest, CancellationToken, UniTask>>();
        }

        public CancellableAsyncProcessHandler(Func<TRequest, CancellationToken, UniTask> process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask ProcessAsync(TRequest request, CancellationToken token)
        {
            return _process(request, token);
        }
    }

    internal sealed class CancellableAsyncProcessHandler<TRequest, TResult> : IAsyncProcessHandler<TRequest, TResult>
    {
        private static readonly TypeId s_typeId;
        private readonly Func<TRequest, CancellationToken, UniTask<TResult>> _process;

        static CancellableAsyncProcessHandler()
        {
            s_typeId = TypeId.Get<Func<TRequest, CancellationToken, UniTask<TResult>>>();
        }

        public CancellableAsyncProcessHandler(Func<TRequest, CancellationToken, UniTask<TResult>> process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TResult> ProcessAsync(TRequest request, CancellationToken token)
        {
            return _process(request, token);
        }
    }
}

#endif