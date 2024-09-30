#if UNITY_ADDRESSABLES

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Module.Core.AddressableKeys;
using Module.Core.Logging;
using Module.Core.Mvvm.ViewBinding;
using Module.Core.Unions;
using Module.Core.Unions.Converters;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Adapters.AddressableKeys
{
    public abstract class AddressablesAdapter<T> : IAdapter
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
                var key = new AddressableKey<T>(new(address));
                var result = key.TryLoad();

                if (result.TryValue(out var asset))
                {
                    return _converter.ToUnionT(asset);
                }

                ErrorFoundNoAsset(typeof(T), address);
            }

            return union;
        }

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
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
        protected sealed override bool TryGetAddressPath(in Union union, out string result)
        {
            return union.TryGetValue(out result);
        }
    }

    public abstract class AddressableKeyUntypedAdapter<T> : AddressablesAdapter<T>
        where T : UnityEngine.Object
    {
        protected sealed override bool TryGetAddressPath(in Union union, out string result)
        {
            var converter = Union<AddressableKey>.GetConverter();

            if (converter.TryGetValue(union, out var key) && key.IsValid)
            {
                result = key.Value;
                return true;
            }

            result = string.Empty;
            return false;
        }
    }

    public abstract class AddressableKeyTypedAdapter<T> : AddressablesAdapter<T>
        where T : UnityEngine.Object
    {
        protected sealed override bool TryGetAddressPath(in Union union, out string result)
        {
            var converter = Union<AddressableKey<T>>.GetConverter();

            if (converter.TryGetValue(union, out var key) && key.IsValid)
            {
                result = key.Value;
                return true;
            }

            result = string.Empty;
            return false;
        }
    }
}

#endif
