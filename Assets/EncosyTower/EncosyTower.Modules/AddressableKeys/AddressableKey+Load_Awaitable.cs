#if !UNITASK && UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EncosyTower.Modules.AddressableKeys
{
    partial record struct AddressableKey<T> : ILoadAsync<T>, ITryLoadAsync<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Awaitable<T> LoadAsync(CancellationToken token = default)
        {
            var result = await TryLoadAsync(token);
            return result.ValueOrDefault();
        }

        public async Awaitable<Option<T>> TryLoadAsync(CancellationToken token = default)
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

                await Awaitable.NextFrameAsync(token);

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
