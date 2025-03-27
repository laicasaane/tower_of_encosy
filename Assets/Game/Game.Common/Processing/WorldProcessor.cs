using System.Runtime.CompilerServices;
using EncosyTower.Processing;
using UnityEngine;

namespace Module.GameCommon.Processing
{
    public static class WorldProcessor
    {
        private static Processor s_instance;

        public static Processor Instance => s_instance ??= new();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitWhenDomainReloadDisabled()
        {
            s_instance?.Dispose();
            s_instance = null;
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProcessHub<TScope> Scope<TScope>(TScope scope)
            => Instance.Scope(scope);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProcessHub<TScope> Scope<TScope>()
            where TScope : struct
            => Instance.Scope<TScope>();
    }
}
