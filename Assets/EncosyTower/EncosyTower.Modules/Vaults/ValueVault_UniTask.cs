#if UNITASK

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace EncosyTower.Modules.Vaults
{
    public sealed partial class ValueVault<TValue>
    {
        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask WaitUntilContains<T>(Id<T> id, CancellationToken token = default)
            => WaitUntilContains(ToId2(id), token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask WaitUntil<T>(Id<T> id, TValue value, CancellationToken token = default)
            => WaitUntil(ToId2(id), value, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<Option<TValue>> TryGetAsync<T>(Id<T> id, CancellationToken token = default)
            => TryGetAsync(ToId2(id), token);

        #region    PRESET_ID
        #endregion =========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask WaitUntilContains(PresetId id, CancellationToken token = default)
            => WaitUntilContains((Id2)id, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask WaitUntil(PresetId id, TValue other, CancellationToken token = default)
            => WaitUntil((Id2)id, other, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<Option<TValue>> TryGetAsync(PresetId id, CancellationToken token = default)
            => TryGetAsync((Id2)id, token);

        #region    ID2
        #endregion ===

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask WaitUntilContains(Id2 id, CancellationToken token = default)
        {
            var map = _map;

            while (map.ContainsKey(id) == false)
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
        public async UniTask WaitUntil(Id2 id, TValue other, CancellationToken token = default)
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

                await UniTask.NextFrame(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<Option<TValue>> TryGetAsync(Id2 id, CancellationToken token = default)
        {
            await WaitUntilContains(id, token);

            return token.IsCancellationRequested
                ? default
                : _map.GetValueOrDefault(id);
        }
    }
}

#endif
