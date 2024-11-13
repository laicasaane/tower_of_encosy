#if !UNITASK && UNITY_6000_0_OR_NEWER

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Modules.Vaults
{
    public sealed partial class ValueVault<TValue>
    {
        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable WaitUntilContains<T>(Id<T> id, CancellationToken token = default)
            => WaitUntilContains(ToId2(id), token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable WaitUntil<T>(Id<T> id, TValue value, CancellationToken token = default)
            => WaitUntil(ToId2(id), value, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable<Option<TValue>> TryGetAsync<T>(Id<T> id, CancellationToken token = default)
            => TryGetAsync(ToId2(id), token);

        #region    PRESET_ID
        #endregion =========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable WaitUntilContains(PresetId id, CancellationToken token = default)
            => WaitUntilContains((Id2)id, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable WaitUntil(PresetId id, TValue other, CancellationToken token = default)
            => WaitUntil((Id2)id, other, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable<Option<TValue>> TryGetAsync(PresetId id, CancellationToken token = default)
            => TryGetAsync((Id2)id, token);

        #region    ID2
        #endregion ===

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Awaitable WaitUntilContains(Id2 id, CancellationToken token = default)
        {
            var map = _map;

            while (map.ContainsKey(id) == false)
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
        public async Awaitable WaitUntil(Id2 id, TValue other, CancellationToken token = default)
        {
            var map = _map;

            while (map.TryGetValue(id, out var value) == false
                || EqualityComparer<TValue>.Default.Equals(value, other) == false
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
        public async Awaitable<Option<TValue>> TryGetAsync(Id2 id, CancellationToken token = default)
        {
            await WaitUntilContains(id, token);

            return token.IsCancellationRequested
                ? default
                : _map.GetValueOrDefault(id);
        }
    }
}

#endif
