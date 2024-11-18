using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.NameKeys
{
    /// <inheritdoc cref="GlobalNameVault{T}"/>
    public static class NameToKey
    {
        /// <inheritdoc cref="GlobalNameVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Capacity<T>()
            => GlobalNameVault<T>.Capacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<T>()
            => GlobalNameVault<T>.Count;

        /// <inheritdoc cref="GlobalNameVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NameKey<T> Get<T>(string name)
            => GlobalNameVault<T>.NameToKey(name);
    }

    /// <inheritdoc cref="GlobalNameVault{T}"/>
    public static class KeyToName
    {
        /// <inheritdoc cref="GlobalNameVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Capacity<T>()
            => GlobalNameVault<T>.Capacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<T>()
            => GlobalNameVault<T>.Count;

        /// <inheritdoc cref="GlobalNameVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Get<T>(NameKey<T> key)
        {
            GlobalNameVault<T>.ThrowIfNotDefined(GlobalNameVault<T>.IsDefined(key), key);
            return GlobalNameVault<T>.KeyToName(key);
        }
    }
}
