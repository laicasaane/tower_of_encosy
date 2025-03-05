#if UNITASK || UNITY_6000_0_OR_NEWER

namespace EncosyTower.Loaders
{
    using System.Threading;
    using EncosyTower.Common;

    public interface ITryLoadAsync<T>
    {
#if UNITASK
        Cysharp.Threading.Tasks.UniTask<Option<T>>
#else
        UnityEngine.Awaitable<Option<T>>
#endif
        TryLoadAsync(CancellationToken token);
    }
}

#endif
