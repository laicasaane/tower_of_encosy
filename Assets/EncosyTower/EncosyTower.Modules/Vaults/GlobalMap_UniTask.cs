#if UNITASK

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace EncosyTower.Modules.Vaults
{
    public static partial class GlobalMap<TKey, TValue>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask WaitUntilContains(TKey key, CancellationToken token = default)
        {
            var map = s_map;

            while (map.ContainsKey(key) == false)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                await UniTask.NextFrame(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask WaitUntil(TKey key, TValue value, CancellationToken token = default)
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

                await UniTask.NextFrame(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<Option<TValue>> TryGetAsync(TKey key, CancellationToken token = default)
        {
            var map = s_map;
            TValue value;

            while (map.TryGetValue(key, out value) == false)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                await UniTask.NextFrame(token);

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
