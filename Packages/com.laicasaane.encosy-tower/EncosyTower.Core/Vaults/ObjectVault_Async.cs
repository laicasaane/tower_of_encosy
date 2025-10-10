#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;

namespace EncosyTower.Vaults
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
    using UnityTaskObject = Cysharp.Threading.Tasks.UniTask<Option<object>>;
#else
    using UnityTask = UnityEngine.Awaitable;
    using UnityTaskObject = UnityEngine.Awaitable<Option<object>>;
#endif

    using UnityObject = UnityEngine.Object;

    partial class ObjectVault<TId>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UnityTask WaitUntilContains(TId id, CancellationToken token = default)
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
        public async
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<T>>
#else
            UnityEngine.Awaitable<Option<T>>
#endif
            TryGetAsync<T>(
              TId id
            , UnityObject context
            , CancellationToken token
        )
        {
            ThrowIfNotReferenceType<T>();

            await WaitUntilContains(id, token);

            if (token.IsCancellationRequested)
            {
                return Option.None;
            }

            return TryCast<T>(id, _map[id], context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UnityTaskObject TryGetAsync(
              TId id
            , UnityObject context
            , CancellationToken token
        )
        {
            await WaitUntilContains(id, token);

            if (token.IsCancellationRequested)
            {
                return Option.None;
            }

            return TryCast(id, _map[id], context);
        }
    }
}

#endif
