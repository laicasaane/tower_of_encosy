#if UNITY_BURST && UNITY_COLLECTIONS

using System.Runtime.CompilerServices;
using Unity.Collections;

namespace EncosyTower.Modules.NameKeys
{
    /// <inheritdoc cref="GlobalFixedNameVault{T}"/>
    public static class FixedNameToKey
    {
        /// <inheritdoc cref="GlobalFixedNameVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Capacity<T>()
            => GlobalFixedNameVault<T>.Capacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<T>()
            => GlobalFixedNameVault<T>.Count;

        /// <inheritdoc cref="GlobalFixedNameVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NameKey<T> Get<T>(string name)
            => GlobalFixedNameVault<T>.NameToKey(name);

        /// <inheritdoc cref="GlobalFixedNameVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NameKey<T> Get<T>(in FixedString32Bytes name)
            => GlobalFixedNameVault<T>.NameToKey(name);
    }

    /// <inheritdoc cref="GlobalFixedNameVault{T}"/>
    public static class KeyToFixedName
    {
        /// <inheritdoc cref="GlobalFixedNameVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Capacity<T>()
            => GlobalFixedNameVault<T>.Capacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<T>()
            => GlobalFixedNameVault<T>.Count;

        /// <inheritdoc cref="GlobalFixedNameVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes Get<T>(NameKey<T> key)
        {
            GlobalFixedNameVault<T>.ThrowIfNotDefined(GlobalFixedNameVault<T>.IsDefined(key), key);
            return GlobalFixedNameVault<T>.KeyToName(key);
        }
    }
}

#endif
