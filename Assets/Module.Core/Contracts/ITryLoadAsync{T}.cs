#if UNITASK

namespace Module.Core
{
    using System.Threading;
    using Cysharp.Threading.Tasks;

    public interface ITryLoadAsync<T>
    {
        UniTask<Option<T>> TryLoadAsync(CancellationToken token);
    }
}

#elif UNITY_6000_0_OR_NEWER

namespace Module.Core
{
    using System.Threading;
    using UnityEngine;

    public interface ITryLoadAsync<T>
    {
        Awaitable<Option<T>> TryLoadAsync(CancellationToken token);
    }
}

#endif
