#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Ids;

namespace EncosyTower.Vaults
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public static partial class GlobalValueVault<TValue>
    {
        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntilContains<T>(Id<T> id, CancellationToken token = default)
            => s_vault.WaitUntilContains(id, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntil<T>(Id<T> id, TValue value, CancellationToken token = default)
            => s_vault.WaitUntil(id, value, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<TValue>>
#else
            UnityEngine.Awaitable<Option<TValue>>
#endif
            TryGetAsync<T>(Id<T> id, CancellationToken token = default)
            => s_vault.TryGetAsync(id, token);

        #region    ID2
        #endregion ===

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntilContains(Id2 id, CancellationToken token = default)
            => s_vault.WaitUntilContains(id, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntil(Id2 id, TValue value, CancellationToken token = default)
            => s_vault.WaitUntil(id, value, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<TValue>>
#else
            UnityEngine.Awaitable<Option<TValue>>
#endif
            TryGetAsync(Id2 id, CancellationToken token = default)
            => s_vault.TryGetAsync(id, token);
    }
}

#endif
