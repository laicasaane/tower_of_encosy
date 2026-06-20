using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Loaders;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.ResourceKeys
{
    using Error = ResourceKeyError;

    partial struct ResourceKey<T> : ILoad<T>, ITryLoad<T>, ILoadOrError<T, Error>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T Load()
            => TryLoad().GetValueOrDefault();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Option<T> TryLoad()
            => LoadOrError().Value;

        public readonly Result<T, Error> LoadOrError()
        {
            if (IsValid == false)
            {
                return Error.InvalidKey((ResourceKey)this);
            }

            try
            {
                var obj = Resources.Load<T>(Value.Value);

                if (obj.IsValid())
                {
                    return obj;
                }

                return Error.InvalidObject((ResourceKey)this);
            }
            catch (Exception ex)
            {
                return Error.Exception((ResourceKey)this, ex);
            }
        }
    }
}
