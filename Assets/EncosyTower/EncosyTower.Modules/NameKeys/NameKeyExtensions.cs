using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.NameKeys
{
    public static class NameKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringGlobal<T>(this NameKey<T> self)
        {
#if UNITY_BURST && UNITY_COLLECTIONS
            if (self.IsFixed)
            {
                GlobalFixedNameVault<T>.ThrowIfNotDefined(GlobalFixedNameVault<T>.IsDefined(self), self);
                return GlobalFixedNameVault<T>.KeyToName(self).ToString();
            }
#endif

            GlobalNameVault<T>.ThrowIfNotDefined(GlobalNameVault<T>.IsDefined(self), self);
            return GlobalNameVault<T>.KeyToName(self);
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
