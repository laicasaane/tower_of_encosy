using System.Runtime.CompilerServices;

namespace Module.Core.NameKeys
{
    public static class NameKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringGlobal<T>(this NameKey<T> self)
        {
#if UNITY_BURST && UNITY_COLLECTIONS
            if (self.IsFixed)
            {
                GlobalFixedNameVault<T>.ThrowIfNotDefined(self);
                return GlobalFixedNameVault<T>.KeyToName(self).ToString();
            }
#endif

            GlobalNameVault<T>.ThrowIfNotDefined(self);
            return GlobalNameVault<T>.KeyToName(self).ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefinedGlobal<T>(this NameKey<T> self)
        {
#if UNITY_BURST && UNITY_COLLECTIONS
            return self.IsFixed ? GlobalFixedNameVault<T>.IsDefined(self) : GlobalNameVault<T>.IsDefined(self);
#else
            return GlobalNameVault<T>.IsDefined(self);
#endif

        }
    }
}
