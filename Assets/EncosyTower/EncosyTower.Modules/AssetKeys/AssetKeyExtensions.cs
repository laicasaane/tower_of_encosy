using System.Runtime.CompilerServices;

namespace EncosyTower.Modules
{
    public static partial class AssetKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetKey Format<T0>(this AssetKey key, T0 arg0)
            => new(string.Format(key.ToString(), arg0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetKey Format<T0, T1>(this AssetKey key, T0 arg0, T1 arg1)
            => new(string.Format(key.ToString(), arg0, arg1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetKey Format<T0, T1, T2>(this AssetKey key, T0 arg0, T1 arg1, T2 arg2)
            => new(string.Format(key.ToString(), arg0, arg1, arg2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetKey<T> Format<T, T0>(this AssetKey<T> key, T0 arg0)
            => new(string.Format(key.ToString(), arg0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetKey<T> Format<T, T0, T1>(this AssetKey<T> key, T0 arg0, T1 arg1)
            => new(string.Format(key.ToString(), arg0, arg1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetKey<T> Format<T, T0, T1, T2>(this AssetKey<T> key, T0 arg0, T1 arg1, T2 arg2)
            => new(string.Format(key.ToString(), arg0, arg1, arg2));

        #region SERIALIZABLE
        #endregion =========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetKey.Serializable Format<T0>(this AssetKey.Serializable key, T0 arg0)
            => new(string.Format(key.Value, arg0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetKey.Serializable Format<T0, T1>(this AssetKey.Serializable key, T0 arg0, T1 arg1)
            => new(string.Format(key.Value, arg0, arg1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetKey.Serializable Format<T0, T1, T2>(this AssetKey.Serializable key, T0 arg0, T1 arg1, T2 arg2)
            => new(string.Format(key.Value, arg0, arg1, arg2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetKey.Serializable<T> Format<T, T0>(this AssetKey.Serializable<T> key, T0 arg0)
            => new(string.Format(key.Value, arg0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetKey.Serializable<T> Format<T, T0, T1>(this AssetKey.Serializable<T> key, T0 arg0, T1 arg1)
            => new(string.Format(key.Value, arg0, arg1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetKey.Serializable<T> Format<T, T0, T1, T2>(this AssetKey.Serializable<T> key, T0 arg0, T1 arg1, T2 arg2)
            => new(string.Format(key.Value, arg0, arg1, arg2));
    }
}
