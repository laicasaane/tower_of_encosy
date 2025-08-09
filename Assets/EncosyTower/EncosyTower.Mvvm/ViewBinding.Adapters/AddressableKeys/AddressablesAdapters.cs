#if UNITY_ADDRESSABLES

using System;
using System.Diagnostics;
using EncosyTower.AddressableKeys;
using EncosyTower.Logging;
using EncosyTower.Unions;
using EncosyTower.Unions.Converters;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Adapters.AddressableKeys
{
    public abstract class AddressablesAdapter<T> : IAdapter
       where T : UnityEngine.Object
    {
        private readonly CachedUnionConverter<T> _assetConverter;

        protected AddressablesAdapter(CachedUnionConverter<T> assetConverter)
        {
            _assetConverter = assetConverter;
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
                var key = new AddressableKey<T>(new(address));
                var result = key.TryLoad();

                if (result.TryGetValue(out var asset))
                {
                    return _assetConverter.ToUnionT(asset);
                }

                ErrorFoundNoAsset(typeof(T), address);
            }

            return union;
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
        protected AddressableStringAdapter(CachedUnionConverter<T> assetConverter) : base(assetConverter) { }

        protected sealed override bool TryGetAddressPath(in Union union, out string result)
        {
            return union.TryGetValue(out result);
        }
    }

    public abstract class AddressableKeyAdapter<T> : AddressablesAdapter<T>
        where T : UnityEngine.Object
    {
        private readonly CachedUnionConverter<AddressableKey> _keyConverter;

        protected AddressableKeyAdapter(CachedUnionConverter<T> assetConverter) : base(assetConverter)
        {
            _keyConverter = CachedUnionConverter<AddressableKey>.Default;
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

#endif
