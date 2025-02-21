using System.Runtime.CompilerServices;

namespace EncosyTower.StringIds
{
    /// <inheritdoc cref="GlobalStringVault{T}"/>
    public static class StringToId
    {
        /// <inheritdoc cref="GlobalStringVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Capacity<T>()
            => GlobalStringVault<T>.Capacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<T>()
            => GlobalStringVault<T>.Count;

        /// <inheritdoc cref="GlobalStringVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringId<T> Get<T>(string str)
            => GlobalStringVault<T>.MakeId(str);
    }

    /// <inheritdoc cref="GlobalStringVault{T}"/>
    public static class IdToString
    {
        /// <inheritdoc cref="GlobalStringVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Capacity<T>()
            => GlobalStringVault<T>.Capacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<T>()
            => GlobalStringVault<T>.Count;

        /// <inheritdoc cref="GlobalStringVault{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Get<T>(StringId<T> key)
        {
            GlobalStringVault<T>.ThrowIfNotDefined(GlobalStringVault<T>.IsDefined(key), key);
            return GlobalStringVault<T>.GetString(key);
        }
    }
}
