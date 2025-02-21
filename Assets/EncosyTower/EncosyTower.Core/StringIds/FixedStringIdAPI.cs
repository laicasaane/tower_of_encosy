#if UNITY_BURST && UNITY_COLLECTIONS

using System.Runtime.CompilerServices;
using Unity.Collections;

namespace EncosyTower.StringIds
{
    /// <inheritdoc cref="GlobalFixedStringVault{T}"/>
    public static class FixedStringToId
    {
        /// <inheritdoc cref="GlobalFixedStringVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Capacity<T>()
            => GlobalFixedStringVault<T>.Capacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<T>()
            => GlobalFixedStringVault<T>.Count;

        /// <inheritdoc cref="GlobalFixedStringVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringId<T> Get<T>(string str)
            => GlobalFixedStringVault<T>.MakeId(str);

        /// <inheritdoc cref="GlobalFixedStringVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringId<T> Get<T>(in FixedString32Bytes str)
            => GlobalFixedStringVault<T>.MakeId(str);
    }

    /// <inheritdoc cref="GlobalFixedStringVault{T}"/>
    public static class IdToFixedString
    {
        /// <inheritdoc cref="GlobalFixedStringVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Capacity<T>()
            => GlobalFixedStringVault<T>.Capacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<T>()
            => GlobalFixedStringVault<T>.Count;

        /// <inheritdoc cref="GlobalFixedStringVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes Get<T>(StringId<T> key)
        {
            GlobalFixedStringVault<T>.ThrowIfNotDefined(GlobalFixedStringVault<T>.IsDefined(key), key);
            return GlobalFixedStringVault<T>.GetString(key);
        }
    }
}

#endif
