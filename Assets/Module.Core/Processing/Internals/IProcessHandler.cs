#if UNITASK || UNITY_6000_0_OR_NEWER

namespace Module.Core.Processing.Internals
{
    internal interface IProcessHandler
    {
        TypeId Id { get; }
    }

    internal interface IProcessHandler<TRequest> : IProcessHandler
    {
        void Process(TRequest request);
    }

    internal interface IProcessHandler<TRequest, TResult> : IProcessHandler
    {
        TResult Process(TRequest request);
    }
}

#endif

#if UNITASK

namespace Module.Core.Processing.Internals
{
    using System.Threading;
    using Cysharp.Threading.Tasks;

    internal interface IAsyncProcessHandler<TRequest> : IProcessHandler
    {
        UniTask ProcessAsync(TRequest request, CancellationToken token);
    }

    internal interface IAsyncProcessHandler<TRequest, TResult> : IProcessHandler
    {
        UniTask<TResult> ProcessAsync(TRequest request, CancellationToken token);
    }
}

#elif UNITY_6000_0_OR_NEWER

namespace Module.Core.Processing.Internals
{
    using System.Threading;
    using UnityEngine;

    internal interface IAsyncProcessHandler<TRequest> : IProcessHandler
    {
        Awaitable ProcessAsync(TRequest request, CancellationToken token);
    }

    internal interface IAsyncProcessHandler<TRequest, TResult> : IProcessHandler
    {
        Awaitable<TResult> ProcessAsync(TRequest request, CancellationToken token);
    }
}

#endif
