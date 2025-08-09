using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Loaders;
using UnityEngine;

namespace EncosyTower.ResourceKeys
{
    partial record struct ResourceKey<T> : ILoad<T>, ITryLoad<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T Load()
            => TryLoad().GetValueOrDefault();

        public readonly Option<T> TryLoad()
        {
            if (IsValid == false) return default;

            try
            {
                var obj = Resources.Load<T>(Value.Value);

                if (obj is { } asset && asset)
                {
                    return obj;
                }
            }
            catch
            {
                // ignored
            }

            return default;
        }
    }
}
