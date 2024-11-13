#if UNITASK || UNITY_6000_0_OR_NEWER

namespace EncosyTower.Modules.Processing.Internals
{
    internal interface IProcessHandler
    {
        TypeId Id { get; }
    }

    internal interface IProcessHandler<in TRequest> : IProcessHandler
    {
        void Process(TRequest request);
    }

    internal interface IProcessHandler<in TRequest, out TResult> : IProcessHandler
    {
        TResult Process(TRequest request);
    }
}

#endif

#if UNITASK

namespace EncosyTower.Modules.Processing.Internals
{
    using System.Threading;
    using Cysharp.Threading.Tasks;

    internal interface IAsyncProcessHandler<in TRequest> : IProcessHandler
    {
        UniTask ProcessAsync(TRequest request, CancellationToken token);
    }

    internal interface IAsyncProcessHandler<in TRequest, TResult> : IProcessHandler
    {
        UniTask<TResult> ProcessAsync(TRequest request, CancellationToken token);
    }
}

#elif UNITY_6000_0_OR_NEWER

namespace EncosyTower.Modules.Processing.Internals
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
