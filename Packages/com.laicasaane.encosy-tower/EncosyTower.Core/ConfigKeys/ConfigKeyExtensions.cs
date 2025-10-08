using System.Runtime.CompilerServices;

namespace EncosyTower.ConfigKeys
{
    public static partial class ConfigKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfigKey Format<T0>(this ConfigKey key, T0 arg0)
            => new(string.Format(key.ToString(), arg0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfigKey Format<T0, T1>(this ConfigKey key, T0 arg0, T1 arg1)
            => new(string.Format(key.ToString(), arg0, arg1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfigKey Format<T0, T1, T2>(this ConfigKey key, T0 arg0, T1 arg1, T2 arg2)
            => new(string.Format(key.ToString(), arg0, arg1, arg2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfigKey<T> Format<T, T0>(this ConfigKey<T> key, T0 arg0)
            => new(string.Format(key.ToString(), arg0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfigKey<T> Format<T, T0, T1>(this ConfigKey<T> key, T0 arg0, T1 arg1)
            => new(string.Format(key.ToString(), arg0, arg1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfigKey<T> Format<T, T0, T1, T2>(this ConfigKey<T> key, T0 arg0, T1 arg1, T2 arg2)
            => new(string.Format(key.ToString(), arg0, arg1, arg2));
    }
}
