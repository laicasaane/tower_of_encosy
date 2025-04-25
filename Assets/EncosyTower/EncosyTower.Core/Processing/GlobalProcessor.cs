#if UNITASK || UNITY_6000_0_OR_NEWER

using UnityEngine;

namespace EncosyTower.Processing
{
    public static class GlobalProcessor
    {
        private static Processor s_instance;

        public static Processor Instance => s_instance ??= new();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InitWhenDomainReloadDisabled()
        {
            s_instance?.Dispose();
            s_instance = null;
        }
#endif
    }
}

#endif
