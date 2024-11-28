using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Vaults
{
#if UNITY_EDITOR
    internal static partial class GlobalValueVaultEditor
    {
        private readonly static System.Collections.Generic.Dictionary<TypeId, IClearable> s_vaults = new();

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            var vaults = s_vaults;

            foreach (var (_, disposable) in vaults)
            {
                disposable?.Clear();
            }
        }

        public static void Register<T>(ValueVault<T> vault)
            where T : struct
        {
            s_vaults.TryAdd((TypeId)TypeId<T>.Value, vault);
        }
    }
#endif

    public static partial class GlobalValueVault<TValue>
        where TValue : struct
    {
        private readonly static ValueVault<TValue> s_vault = new();

#if UNITY_EDITOR
        static GlobalValueVault()
        {
            GlobalValueVaultEditor.Register(s_vault);
        }
#endif

        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(Id<T> id)
            => s_vault.Contains(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemove<T>(Id<T> id)
            => s_vault.TryRemove(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet<T>(Id<T> id, out TValue value)
            => s_vault.TryGet(id, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySet<T>(Id<T> id, TValue value)
            => s_vault.TrySet(id, value);

        #region    PRESET_ID
        #endregion =========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(PresetId id)
            => s_vault.Contains(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemove(PresetId id)
            => s_vault.TryRemove(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet(PresetId id, out TValue value)
            => s_vault.TryGet(id, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySet(PresetId id, TValue value)
            => s_vault.TrySet(id, value);

        #region    ID2
        #endregion ===

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(Id2 id)
            => s_vault.Contains(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemove(Id2 id)
            => s_vault.TryRemove(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet(Id2 id, out TValue value)
            => s_vault.TryGet(id, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySet(Id2 id, TValue value)
            => s_vault.TrySet(id, value);
    }
}
