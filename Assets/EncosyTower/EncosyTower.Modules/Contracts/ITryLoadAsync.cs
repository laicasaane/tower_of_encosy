#if UNITASK

namespace EncosyTower.Modules
{
    using System.Threading;
    using Cysharp.Threading.Tasks;

    public interface ITryLoadAsync<T>
    {
        UniTask<Option<T>> TryLoadAsync(CancellationToken token);
    }
}

#elif UNITY_6000_0_OR_NEWER

namespace EncosyTower.Modules
{
    using System.Threading;
    using UnityEngine;

    public interface ITryLoadAsync<T>
    {
        Awaitable<Option<T>> TryLoadAsync(CancellationToken token);
    }
}

#endif
