#if !UNITASK && UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Modules.Vaults
{
    using UnityObject = UnityEngine.Object;

    public sealed partial class ObjectVault
    {
        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable WaitUntilContains<T>(Id<T> id, CancellationToken token = default)
            => WaitUntilContains(ToId2(id), token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable<Option<T>> TryGetAsync<T>(Id<T> id, UnityObject context = null, CancellationToken token = default)
            => TryGetAsync<T>(ToId2(id), context, token);

        #region    PRESET_ID
        #endregion =========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable WaitUntilContains(PresetId id, CancellationToken token = default)
            => WaitUntilContains((Id2)id, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaitable<Option<T>> TryGetAsync<T>(PresetId id, UnityObject context = null, CancellationToken token = default)
            => TryGetAsync<T>((Id2)id, context, token);

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
        public async Awaitable<Option<T>> TryGetAsync<T>(
              Id2 id
            , UnityObject context
            , CancellationToken token
        )
        {
            ThrowIfNotReferenceType<T>();

            await WaitUntilContains(id, token);

            if (token.IsCancellationRequested)
            {
                return default;
            }

            return TryCast<T>(id, _map[id], context);
        }
    }
}

#endif
