#if UNITY_ADDRESSABLES

using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Loaders;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EncosyTower.AddressableKeys
{
    partial struct AddressableKey<T> : ILoad<T>, ITryLoad<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T Load()
            => TryLoad().GetValueOrDefault();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly (T, AsyncOperationHandle<T>) LoadGetHandle()
            => TryLoadGetHandle().GetValueOrDefault();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Option<T> TryLoad()
        {
            var result = TryLoadGetHandle();
            return Option.SomeIf(result.HasValue, result.GetValueOrDefault().Item1);
        }

        public readonly Option<(T, AsyncOperationHandle<T>)> TryLoadGetHandle()
        {
            if (IsValid == false)
            {
                return Option.None;
            }

            var handle = Addressables.LoadAssetAsync<T>(Value.Value);
            var asset = handle.WaitForCompletion();

            if (handle.IsValid() == false || handle.Status != AsyncOperationStatus.Succeeded)
            {
                handle.TryRelease();
                return Option.None;
            }

            if (asset is UnityEngine.Object obj && obj || asset != null)
            {
                return (asset, handle);
            }

            handle.TryRelease();
            return Option.None;
        }
    }
}

#endif
