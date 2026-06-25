#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

namespace EncosyTower.Vaults
{
    public static class GlobalSingletonVault
    {
        private static SingletonVault<object > s_vault = new();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode, UnityEngine.Scripting.Preserve]
        private static void InitWhenDomainReloadDisabled()
        {
            s_vault?.Dispose();
            s_vault = new();
        }
#endif

        public static bool Contains<T>()
            where T : class
        {
            return s_vault.Contains<T>();
        }

        public static bool Contains<T>(T instance)
            where T : class
        {
            return s_vault.Contains(instance);
        }

        public static bool TryAdd<T>()
            where T : class, new()
        {
            return s_vault.TryAdd<T>();
        }

        public static bool TryAdd<T>(T instance)
            where T : class
        {
            return s_vault.TryAdd(instance);
        }

        public static bool TryGetOrAdd<T>(out T instance)
            where T : class, new()
        {
            return s_vault.TryGetOrAdd<T>(out instance);
        }

        public static bool TryGet<T>(out T instance)
            where T : class
        {
            return s_vault.TryGet(out instance);
        }
    }
}
