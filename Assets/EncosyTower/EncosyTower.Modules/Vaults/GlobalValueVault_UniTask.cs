#if UNITASK

using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace EncosyTower.Modules.Vaults
{
    public static partial class GlobalValueVault<TValue>
    {
        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask WaitUntilContains<T>(Id<T> id, CancellationToken token = default)
            => s_vault.WaitUntilContains(id, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask WaitUntil<T>(Id<T> id, TValue value, CancellationToken token = default)
            => s_vault.WaitUntil(id, value, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask<Option<TValue>> TryGetAsync<T>(Id<T> id, CancellationToken token = default)
            => s_vault.TryGetAsync(id, token);

        #region    PRESET_ID
        #endregion =========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask WaitUntilContains(PresetId id, CancellationToken token = default)
            => s_vault.WaitUntilContains(id, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask WaitUntil(PresetId id, TValue value, CancellationToken token = default)
            => s_vault.WaitUntil(id, value, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask<Option<TValue>> TryGetAsync(PresetId id, CancellationToken token = default)
            => s_vault.TryGetAsync(id, token);

        #region    ID2
        #endregion ===

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask WaitUntilContains(Id2 id, CancellationToken token = default)
            => s_vault.WaitUntilContains(id, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask WaitUntil(Id2 id, TValue value, CancellationToken token = default)
            => s_vault.WaitUntil(id, value, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask<Option<TValue>> TryGetAsync(Id2 id, CancellationToken token = default)
            => s_vault.TryGetAsync(id, token);
    }
}

#endif
