#if !UNITASK && UNITY_6000_0_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Modules.Processing.Internals.Async
{
    internal sealed class AsyncProcessHandler<TRequest> : IAsyncProcessHandler<TRequest>
    {
        private static readonly TypeId s_typeId;
        private readonly Func<TRequest, Awaitable> _process;

        static AsyncProcessHandler()
        {
            s_typeId = TypeId.Get<Func<TRequest, Awaitable>>();
        }

        public AsyncProcessHandler(Func<TRequest, Awaitable> process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable ProcessAsync(TRequest request, CancellationToken token)
        {
            return _process(request);
        }
    }

    internal sealed class AsyncProcessHandler<TRequest, TResult> : IAsyncProcessHandler<TRequest, TResult>
    {
        private static readonly TypeId s_typeId;
        private readonly Func<TRequest, Awaitable<TResult>> _process;

        static AsyncProcessHandler()
        {
            s_typeId = TypeId.Get<Func<TRequest, Awaitable<TResult>>>();
        }

        public AsyncProcessHandler(Func<TRequest, Awaitable<TResult>> process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable<TResult> ProcessAsync(TRequest request, CancellationToken token)
        {
            return _process(request);
        }
    }

    internal sealed class CancellableAsyncProcessHandler<TRequest> : IAsyncProcessHandler<TRequest>
    {
        private static readonly TypeId s_typeId;
        private readonly Func<TRequest, CancellationToken, Awaitable> _process;

        static CancellableAsyncProcessHandler()
        {
            s_typeId = TypeId.Get<Func<TRequest, CancellationToken, Awaitable>>();
        }

        public CancellableAsyncProcessHandler(Func<TRequest, CancellationToken, Awaitable> process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable ProcessAsync(TRequest request, CancellationToken token)
        {
            return _process(request, token);
        }
    }

    internal sealed class CancellableAsyncProcessHandler<TRequest, TResult> : IAsyncProcessHandler<TRequest, TResult>
    {
        private static readonly TypeId s_typeId;
        private readonly Func<TRequest, CancellationToken, Awaitable<TResult>> _process;

        static CancellableAsyncProcessHandler()
        {
            s_typeId = TypeId.Get<Func<TRequest, CancellationToken, Awaitable<TResult>>>();
        }

        public CancellableAsyncProcessHandler(Func<TRequest, CancellationToken, Awaitable<TResult>> process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable<TResult> ProcessAsync(TRequest request, CancellationToken token)
        {
            return _process(request, token);
        }
    }
}

#endif
