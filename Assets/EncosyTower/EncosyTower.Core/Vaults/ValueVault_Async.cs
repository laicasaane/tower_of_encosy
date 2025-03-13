#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.Tasks;

namespace EncosyTower.Vaults
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public sealed partial class ValueVault<TValue>
    {
        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityTask WaitUntilContains<T>(Id<T> id, CancellationToken token = default)
            => WaitUntilContains(ToId2(id), token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityTask WaitUntil<T>(Id<T> id, TValue value, CancellationToken token = default)
            => WaitUntil(ToId2(id), value, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<TValue>>
#else
            UnityEngine.Awaitable<Option<TValue>>
#endif
            TryGetAsync<T>(Id<T> id, CancellationToken token = default)
            => TryGetAsync(ToId2(id), token);

        #region    ID2
        #endregion ===

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UnityTask WaitUntilContains(Id2 id, CancellationToken token = default)
        {
            var map = _map;

            while (map.ContainsKey(id) == false)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                await UnityTasks.NextFrameAsync(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UnityTask WaitUntil(Id2 id, TValue other, CancellationToken token = default)
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

                await UnityTasks.NextFrameAsync(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<TValue>>
#else
            UnityEngine.Awaitable<Option<TValue>>
#endif
            TryGetAsync(Id2 id, CancellationToken token = default)
        {
            await WaitUntilContains(id, token);

            return token.IsCancellationRequested
                ? default(Option<TValue>)
                : _map.GetValueOrDefault(id);
        }
    }
}

#endif
