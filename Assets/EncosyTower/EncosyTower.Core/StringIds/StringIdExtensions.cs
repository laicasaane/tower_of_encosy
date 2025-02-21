using System.Runtime.CompilerServices;

namespace EncosyTower.StringIds
{
    public static class StringIdExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringGlobal<T>(this StringId<T> self)
        {
#if UNITY_BURST && UNITY_COLLECTIONS
            if (self.IsFixed)
            {
                GlobalFixedStringVault<T>.ThrowIfNotDefined(GlobalFixedStringVault<T>.IsDefined(self), self);
                return GlobalFixedStringVault<T>.GetString(self).ToString();
            }
#endif

            GlobalStringVault<T>.ThrowIfNotDefined(GlobalStringVault<T>.IsDefined(self), self);
            return GlobalStringVault<T>.GetString(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefinedGlobal<T>(this StringId<T> self)
        {
#if UNITY_BURST && UNITY_COLLECTIONS
            return self.IsFixed ? GlobalFixedStringVault<T>.IsDefined(self) : GlobalStringVault<T>.IsDefined(self);
#else
            return GlobalStringVault<T>.IsDefined(self);
#endif

        }
    }
}
