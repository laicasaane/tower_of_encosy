#if UNITY_ADDRESSABLES
#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Loaders;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using EncosyTower.Tasks;

namespace EncosyTower.AddressableKeys
{
    partial struct AddressableKey<T> : ILoadAsync<T>, ITryLoadAsync<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<T>
#else
            UnityEngine.Awaitable<T>
#endif
            LoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadAsync(token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<(T, AsyncOperationHandle<T>)>
#else
            UnityEngine.Awaitable<T>
#endif
            LoadGetHandleAsync(CancellationToken token = default)
        {
            var result = await TryLoadGetHandleAsync(token);
            return result.GetValueOrDefault();
        }

        public readonly async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<T>>
#else
            UnityEngine.Awaitable<Option<T>>
#endif
            TryLoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadGetHandleAsync(token);
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Item1);
        }

        public readonly async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<(T, AsyncOperationHandle<T>)>>
#else
            UnityEngine.Awaitable<Option<T>>
#endif
            TryLoadGetHandleAsync(CancellationToken token = default)
        {
            if (IsValid == false)
            {
                return Option.None;
            }

            var handle = Addressables.LoadAssetAsync<T>(Value.Value);

            if (handle.IsValid() == false)
            {
                handle.TryRelease();
                return Option.None;
            }

            while (handle.IsDone == false)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                await UnityTasks.NextFrameAsync(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }

            if (token.IsCancellationRequested)
            {
                handle.TryRelease();
                return Option.None;
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                handle.TryRelease();
                return Option.None;
            }

            var asset = handle.Result;

            if ((asset is UnityEngine.Object obj && obj) || asset != null)
            {
                return (asset, handle);
            }

            handle.TryRelease();
            return Option.None;
        }
    }
}

#endif
#endif
