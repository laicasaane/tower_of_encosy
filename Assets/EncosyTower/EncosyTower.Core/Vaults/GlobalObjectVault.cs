using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Ids;

namespace EncosyTower.Vaults
{
    public static partial class GlobalObjectVault
    {
        private static ObjectVault s_vault = new();

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitWhenDomainReloadDisabled()
        {
            s_vault?.Dispose();
            s_vault = new();
        }
#endif

        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(Id<T> id)
            => s_vault.Contains(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAdd<T>(Id<T> id, [NotNull] T obj)
            => s_vault.TryAdd(id, obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemove<T>(Id<T> id, out T obj)
            => s_vault.TryRemove(id, out obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet<T>(Id<T> id, out T obj)
            => s_vault.TryGet<T>(id, out obj);

        #region    ID2
        #endregion ===

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(Id2 id)
            => s_vault.Contains(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAdd<T>(Id2 id, [NotNull] T obj)
            => s_vault.TryAdd(id, obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemove<T>(Id2 id, out T obj)
            => s_vault.TryRemove(id, out obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet<T>(Id2 id, out T obj)
            => s_vault.TryGet<T>(id, out obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet(Id2 id, out object obj)
            => s_vault.TryGet(id, out obj);
    }
}
