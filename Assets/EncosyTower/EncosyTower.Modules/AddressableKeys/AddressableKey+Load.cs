#if UNITY_ADDRESSABLES

using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;

namespace EncosyTower.Modules.AddressableKeys
{
    partial record struct AddressableKey<T> : ILoad<T>, ITryLoad<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Load()
            => TryLoad().ValueOrDefault();

        public Option<T> TryLoad()
        {
            if (IsValid == false) return default;

            var handle = Addressables.LoadAssetAsync<T>(Value.Value);
            var asset = handle.WaitForCompletion();

            return (asset is UnityEngine.Object obj && obj) || asset != null
                ? asset : default;
        }
    }
}

#endif
