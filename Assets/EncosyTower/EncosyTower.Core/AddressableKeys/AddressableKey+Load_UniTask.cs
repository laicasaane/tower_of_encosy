#if UNITY_ADDRESSABLES
#if UNITASK

using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.Loaders;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EncosyTower.AddressableKeys
{
    partial record struct AddressableKey<T> : ILoadAsync<T>, ITryLoadAsync<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<T> LoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadAsync(token);
            return result.ValueOrDefault();
        }

        public async UniTask<Option<T>> TryLoadAsync(CancellationToken token = default)
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

                await UniTask.NextFrame(token);

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
                ? asset : default;
        }
    }
}

#endif
#endif
