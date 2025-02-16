#if UNITASK

namespace EncosyTower.Loaders
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using EncosyTower.Common;

    public interface ITryLoadAsync<T>
    {
        UniTask<Option<T>> TryLoadAsync(CancellationToken token);
    }
}

#elif UNITY_6000_0_OR_NEWER

namespace EncosyTower.Loaders
{
    using System.Threading;
    using EncosyTower.Common;
    using UnityEngine;

    public interface ITryLoadAsync<T>
    {
        Awaitable<Option<T>> TryLoadAsync(CancellationToken token);
    }
}

#endif
