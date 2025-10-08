using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Loaders;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.ResourceKeys
{
    partial struct ResourceKey<T> : ILoad<T>, ITryLoad<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T Load()
            => TryLoad().GetValueOrDefault();

        public readonly Option<T> TryLoad()
        {
            if (IsValid == false) return Option.None;

            try
            {
                var obj = Resources.Load<T>(Value.Value);

                if (obj.IsValid())
                {
                    return obj;
                }
            }
            catch
            {
                // ignored
            }

            return Option.None;
        }
    }
}
