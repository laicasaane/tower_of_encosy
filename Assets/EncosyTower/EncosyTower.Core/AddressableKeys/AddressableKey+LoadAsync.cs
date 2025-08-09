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
    partial record struct AddressableKey<T> : ILoadAsync<T>, ITryLoadAsync<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async
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

        public async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<T>>
#else
            UnityEngine.Awaitable<Option<T>>
#endif
            TryLoadAsync(CancellationToken token = default)
        {
            if (IsValid == false) return default;

            var handle = Addressables.LoadAssetAsync<T>(Value.Value);

            if (handle.IsValid() == false)
            {
                return default;
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
                return default;
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                return default;
            }

            var asset = handle.Result;

            return (asset is UnityEngine.Object obj && obj) || asset != null
                ? asset : default(Option<T>);
        }
    }
}

#endif
#endif
