using System;
using System.Diagnostics;
using EncosyTower.Logging;
using EncosyTower.ResourceKeys;
using EncosyTower.Variants;
using EncosyTower.Variants.Converters;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Adapters.ResourceKeys
{
    public abstract class ResourcesAdapter<T> : IAdapter
       where T : UnityEngine.Object
    {
        private readonly CachedVariantConverter<T> _converter;

        protected ResourcesAdapter(CachedVariantConverter<T> converter)
        {
            _converter = converter;
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
                var key = new ResourceKey<T>(new(address));
                var result = key.TryLoad();

                if (result.TryGetValue(out var asset))
                {
                    return _converter.ToVariantT(asset);
                }

                ErrorFoundNoAsset(typeof(T), address);
            }

            return variant;
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorFoundNoAsset(Type type, string address)
        {
            DevLoggerAPI.LogErrorFormat("Cannot find Resource Asset of type {0} by address {1}"
                , type.Name
                , address
            );
        }
    }

    public abstract class ResourceStringAdapter<T> : ResourcesAdapter<T>
        where T : UnityEngine.Object
    {
        protected ResourceStringAdapter(CachedVariantConverter<T> converter) : base(converter) { }

        protected sealed override bool TryGetAddressPath(in Variant variant, out string result)
        {
            return variant.TryGetValue(out result);
        }
    }

    public abstract class ResourceKeyAdapter<T> : ResourcesAdapter<T>
        where T : UnityEngine.Object
    {
        private readonly CachedVariantConverter<ResourceKey> _keyConverter;

        protected ResourceKeyAdapter(CachedVariantConverter<T> converter) : base(converter)
        {
            _keyConverter = CachedVariantConverter<ResourceKey>.Default;
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
