#if UNITASK || UNITY_6000_0_OR_NEWER

namespace EncosyTower.Loaders
{
    using System.Threading;
    using EncosyTower.Common;

    public interface ILoadOrErrorAsync<TValue, TError>
    {
#if UNITASK
        Cysharp.Threading.Tasks.UniTask<Result<TValue, TError>>
#else
        UnityEngine.Awaitable<Result<TValue, TError>>
#endif
        LoadOrErrorAsync(CancellationToken token);
    }
}

#endif
