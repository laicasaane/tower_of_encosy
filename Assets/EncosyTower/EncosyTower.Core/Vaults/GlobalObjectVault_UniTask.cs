#if UNITASK

using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.Ids;

namespace EncosyTower.Vaults
{
    using UnityObject = UnityEngine.Object;

    public static partial class GlobalObjectVault
    {
        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask WaitUntilContains<T>(Id<T> id, CancellationToken token = default)
            => s_vault.WaitUntilContains(id, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask<Option<T>> TryGetAsync<T>(Id<T> id, UnityObject context = null, CancellationToken token = default)
            => s_vault.TryGetAsync(id, context, token);

        #region    ID2
        #endregion ===

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask WaitUntilContains<T>(Id2 id, CancellationToken token = default)
            => s_vault.WaitUntilContains(id, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask<Option<T>> TryGetAsync<T>(Id2 id, UnityObject context = null, CancellationToken token = default)
            => s_vault.TryGetAsync<T>(id, context, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask<Option<object>> TryGetAsync(Id2 id, UnityObject context = null, CancellationToken token = default)
            => s_vault.TryGetAsync(id, context, token);
    }
}

#endif
