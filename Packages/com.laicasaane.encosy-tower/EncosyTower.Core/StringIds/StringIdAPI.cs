#if UNITY_COLLECTIONS

using System.Runtime.CompilerServices;

namespace EncosyTower.StringIds
{
    /// <inheritdoc cref="GlobalStringVault"/>
    public static class StringToId
    {
        /// <inheritdoc cref="GlobalStringVault"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Capacity()
            => GlobalStringVault.Capacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count()
            => GlobalStringVault.Count;

        /// <inheritdoc cref="GlobalStringVault"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringId Get(in UnmanagedString str)
            => GlobalStringVault.GetOrMakeId(str);

        /// <inheritdoc cref="GlobalStringVault"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringId Get(in string str)
            => GlobalStringVault.GetOrMakeId(str);
    }

    /// <inheritdoc cref="GlobalStringVault"/>
    public static class IdToString
    {
        /// <inheritdoc cref="GlobalStringVault"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Capacity()
            => GlobalStringVault.Capacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count()
            => GlobalStringVault.Count;

        /// <inheritdoc cref="GlobalStringVault"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnmanagedString GetUnmanaged(StringId id)
        {
            GlobalStringVault.ThrowIfNotDefined(GlobalStringVault.IsDefined(id), id);
            return GlobalStringVault.GetUnmanagedString(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetManaged(StringId id)
        {
            GlobalStringVault.ThrowIfNotDefined(GlobalStringVault.IsDefined(id), id);
            return GlobalStringVault.GetManagedString(id);
        }
    }
}

#endif
