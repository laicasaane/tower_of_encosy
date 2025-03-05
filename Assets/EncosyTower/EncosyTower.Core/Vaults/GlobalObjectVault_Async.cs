#if UNITASK

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Ids;

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

    public static partial class GlobalObjectVault
    {
        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntilContains<T>(Id<T> id, CancellationToken token = default)
            => s_vault.WaitUntilContains(id, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<T>>
#else
            UnityEngine.Awaitable<Option<T>>
#endif
            TryGetAsync<T>(Id<T> id, UnityObject context = null, CancellationToken token = default)
            => s_vault.TryGetAsync(id, context, token);

        #region    ID2
        #endregion ===

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntilContains<T>(Id2 id, CancellationToken token = default)
            => s_vault.WaitUntilContains(id, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if UNITASK
            Cysharp.Threading.Tasks.UniTask<Option<T>>
#else
            UnityEngine.Awaitable<Option<T>>
#endif
            TryGetAsync<T>(Id2 id, UnityObject context = null, CancellationToken token = default)
            => s_vault.TryGetAsync<T>(id, context, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTaskObject TryGetAsync(Id2 id, UnityObject context = null, CancellationToken token = default)
            => s_vault.TryGetAsync(id, context, token);
    }
}

#endif
