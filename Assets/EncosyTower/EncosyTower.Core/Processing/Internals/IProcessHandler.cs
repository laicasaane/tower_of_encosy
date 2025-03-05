#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Threading;
using EncosyTower.Types;

namespace EncosyTower.Processing.Internals
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

    internal interface IAsyncProcessHandler<in TRequest> : IProcessHandler
    {
#if UNITASK
        Cysharp.Threading.Tasks.UniTask
#else
        UnityEngine.Awaitable
#endif
        ProcessAsync(TRequest request, CancellationToken token);
    }

    internal interface IAsyncProcessHandler<in TRequest, TResult> : IProcessHandler
    {
#if UNITASK
        Cysharp.Threading.Tasks.UniTask<TResult>
#else
        UnityEngine.Awaitable<TResult>
#endif
        ProcessAsync(TRequest request, CancellationToken token);
    }
}

#endif
