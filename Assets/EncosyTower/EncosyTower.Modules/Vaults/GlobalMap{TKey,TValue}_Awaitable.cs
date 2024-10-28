#if !UNITASK && UNITY_6000_0_OR_NEWER

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Modules.Vaults
{
    public static partial class GlobalMap<TKey, TValue>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Awaitable WaitUntilContains(TKey key, CancellationToken token = default)
        {
            var map = s_map;

            while (map.ContainsKey(key) == false)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                await Awaitable.NextFrameAsync(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Awaitable WaitUntil(TKey key, TValue value, CancellationToken token = default)
        {
            var map = s_map;

            while (map.TryGetValue(key, out var result) == false
                || EqualityComparer<TValue>.Default.Equals(result, value) == false
            )
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                await Awaitable.NextFrameAsync(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Awaitable<Option<TValue>> TryGetAsync(TKey key, CancellationToken token = default)
        {
            var map = s_map;
            TValue value;

            while (map.TryGetValue(key, out value) == false)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                await Awaitable.NextFrameAsync(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }

            if (token.IsCancellationRequested)
            {
                return default;
            }

            return new(value);
        }
    }
}

#endif
