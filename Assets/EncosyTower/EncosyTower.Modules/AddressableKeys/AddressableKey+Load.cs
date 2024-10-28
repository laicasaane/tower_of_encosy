#if UNITY_ADDRESSABLES

using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;

namespace EncosyTower.Modules.AddressableKeys
{
    partial record struct AddressableKey<T> : ILoad<T>, ITryLoad<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T Load()
            => TryLoad().ValueOrDefault();

        public readonly Option<T> TryLoad()
        {
            if (IsValid == false) return default;

            var handle = Addressables.LoadAssetAsync<T>(Value);
            var asset = handle.WaitForCompletion();

            if ((asset is UnityEngine.Object obj && obj) || asset != null)
            {
                return asset;
            }

            return default;
        }
    }
}

#endif
