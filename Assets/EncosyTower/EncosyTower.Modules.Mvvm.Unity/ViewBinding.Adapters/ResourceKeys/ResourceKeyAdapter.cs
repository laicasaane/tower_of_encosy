using System;
using System.Diagnostics;
using EncosyTower.Modules.Logging;
using EncosyTower.Modules.Unions;
using EncosyTower.Modules.Unions.Converters;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Adapters.ResourceKeys
{
    public abstract class ResourceKeyAdapter<T> : IAdapter
       where T : UnityEngine.Object
    {
        private readonly CachedUnionConverter<T> _converter = new();

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

                if (result.TryValue(out var asset))
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

    public abstract class ResourceStringAdapter<T> : ResourceKeyAdapter<T>
        where T : UnityEngine.Object
    {
        protected sealed override bool TryGetAddressPath(in Union union, out string result)
        {
            return union.TryGetValue(out result);
        }
    }

    public abstract class ResourceKeyUntypedAdapter<T> : ResourceKeyAdapter<T>
        where T : UnityEngine.Object
    {
        protected sealed override bool TryGetAddressPath(in Union union, out string result)
        {
            var converter = Union<ResourceKey>.GetConverter();

            if (converter.TryGetValue(union, out var key) && key.IsValid)
            {
                result = (string)key.Value;
                return true;
            }

            result = string.Empty;
            return false;
        }
    }

    public abstract class ResourceKeyTypedAdapter<T> : ResourceKeyAdapter<T>
        where T : UnityEngine.Object
    {
        protected sealed override bool TryGetAddressPath(in Union union, out string result)
        {
            var converter = Union<ResourceKey<T>>.GetConverter();

            if (converter.TryGetValue(union, out var key) && key.IsValid)
            {
                result = (string)key.Value;
                return true;
            }

            result = string.Empty;
            return false;
        }
    }
}
