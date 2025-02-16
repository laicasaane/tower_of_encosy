#if UNITASK

namespace EncosyTower.Loaders
{
    using System.Threading;
    using Cysharp.Threading.Tasks;

    public interface ILoadAsync<T>
    {
        UniTask<T> LoadAsync(CancellationToken token);
    }
}

#elif UNITY_6000_0_OR_NEWER

namespace EncosyTower.Loaders
{
    using System.Threading;
    using UnityEngine;

    public interface ILoadAsync<T>
    {
        Awaitable<T> LoadAsync(CancellationToken token);
    }
}

#endif
