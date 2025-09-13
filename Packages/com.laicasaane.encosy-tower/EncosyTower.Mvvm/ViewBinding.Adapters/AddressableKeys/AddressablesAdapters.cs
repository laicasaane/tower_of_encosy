#if UNITY_ADDRESSABLES

using System;
using System.Diagnostics;
using EncosyTower.AddressableKeys;
using EncosyTower.Logging;
using EncosyTower.Variants;
using EncosyTower.Variants.Converters;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Adapters.AddressableKeys
{
    public abstract class AddressablesAdapter<T> : IAdapter
       where T : UnityEngine.Object
    {
        private readonly CachedVariantConverter<T> _assetConverter;

        protected AddressablesAdapter(CachedVariantConverter<T> assetConverter)
        {
            _assetConverter = assetConverter;
        }

        protected virtual bool TryGetAddressPath(in Variant variant, out string result)
        {
            result = string.Empty;
            return false;
        }

        public Variant Convert(in Variant variant)
        {
            if (TryGetAddressPath(variant, out var address))
            {
                var key = new AddressableKey<T>(new(address));
                var result = key.TryLoad();

                if (result.TryGetValue(out var asset))
                {
                    return _assetConverter.ToVariantT(asset);
                }

                ErrorFoundNoAsset(typeof(T), address);
            }

            return variant;
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorFoundNoAsset(Type type, string address)
        {
            DevLoggerAPI.LogErrorFormat("Cannot find Addressable Asset of type {0} by address {1}"
                , type.Name
                , address
            );
        }
    }

    public abstract class AddressableStringAdapter<T> : AddressablesAdapter<T>
        where T : UnityEngine.Object
    {
        protected AddressableStringAdapter(CachedVariantConverter<T> assetConverter) : base(assetConverter) { }

        protected sealed override bool TryGetAddressPath(in Variant variant, out string result)
        {
            return variant.TryGetValue(out result);
        }
    }

    public abstract class AddressableKeyAdapter<T> : AddressablesAdapter<T>
        where T : UnityEngine.Object
    {
        private readonly CachedVariantConverter<AddressableKey> _keyConverter;

        protected AddressableKeyAdapter(CachedVariantConverter<T> assetConverter) : base(assetConverter)
        {
            _keyConverter = CachedVariantConverter<AddressableKey>.Default;
        }

        protected sealed override bool TryGetAddressPath(in Variant variant, out string result)
        {
            if (_keyConverter.TryGetValue(variant, out var key) && key.IsValid)
            {
                result = (string)key.Value;
                return true;
            }

            result = string.Empty;
            return false;
        }
    }
}

#endif
