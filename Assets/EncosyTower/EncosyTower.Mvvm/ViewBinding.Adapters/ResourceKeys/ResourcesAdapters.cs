using System;
using System.Diagnostics;
using EncosyTower.Logging;
using EncosyTower.ResourceKeys;
using EncosyTower.Unions;
using EncosyTower.Unions.Converters;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Adapters.ResourceKeys
{
    public abstract class ResourcesAdapter<T> : IAdapter
       where T : UnityEngine.Object
    {
        private readonly CachedUnionConverter<T> _converter;

        protected ResourcesAdapter(CachedUnionConverter<T> converter)
        {
            _converter = converter;
        }

        protected virtual bool TryGetAddressPath(in Union union, out string result)
        {
            result = string.Empty;
            return false;
        }

        public Union Convert(in Union union)
        {
            if (TryGetAddressPath(union, out var address))
            {
                var key = new ResourceKey<T>(new(address));
                var result = key.TryLoad();

                if (result.TryGetValue(out var asset))
                {
                    return _converter.ToUnionT(asset);
                }

                ErrorFoundNoAsset(typeof(T), address);
            }

            return union;
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
        protected ResourceStringAdapter(CachedUnionConverter<T> converter) : base(converter) { }

        protected sealed override bool TryGetAddressPath(in Union union, out string result)
        {
            return union.TryGetValue(out result);
        }
    }

    public abstract class ResourceKeyAdapter<T> : ResourcesAdapter<T>
        where T : UnityEngine.Object
    {
        private readonly CachedUnionConverter<ResourceKey> _keyConverter;

        protected ResourceKeyAdapter(CachedUnionConverter<T> converter) : base(converter)
        {
            _keyConverter = CachedUnionConverter<ResourceKey>.Default;
        }

        protected sealed override bool TryGetAddressPath(in Union union, out string result)
        {
            if (_keyConverter.TryGetValue(union, out var key) && key.IsValid)
            {
                result = (string)key.Value;
                return true;
            }

            result = string.Empty;
            return false;
        }
    }
}
