#if UNITASK || UNITY_6000_0_OR_NEWER

namespace EncosyTower.Loaders
{
    using System.Threading;

    public interface ILoadAsync<T>
    {
#if UNITASK
        Cysharp.Threading.Tasks.UniTask<T>
#else
        UnityEngine.Awaitable<T>
#endif
        LoadAsync(CancellationToken token);
    }
}

#endif
