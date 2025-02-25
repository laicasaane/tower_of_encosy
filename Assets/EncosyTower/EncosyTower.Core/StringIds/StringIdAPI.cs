#if UNITY_COLLECTIONS
#define __ENCOSY_SHARED_STRING_VAULT__
#endif

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.StringIds
{
#if __ENCOSY_SHARED_STRING_VAULT__
    using String = Unity.Collections.FixedString64Bytes;
#else
    using String = System.String;
#endif

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
        public static StringId Get(in String str)
            => GlobalStringVault.MakeId(str);
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
        public static String Get(StringId key)
        {
            GlobalStringVault.ThrowIfNotDefined(GlobalStringVault.IsDefined(key), key);
            return GlobalStringVault.GetString(key);
        }
    }

    public static class StringIdExtensions
    {
        private static readonly Dictionary<StringId, string> s_managedStringMap = new(256);

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            s_managedStringMap.Clear();
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefinedInGlobalVault<T>(this StringId<T> self)
            => IsDefinedInGlobalVault(self.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefinedInGlobalVault(this StringId self)
            => GlobalStringVault.IsDefined(self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringGlobal<T>(this StringId<T> self)
            => ToStringGlobal(self.Id);

#if !__ENCOSY_SHARED_STRING_VAULT__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToStringGlobal(this StringId self)
        {
            GlobalStringVault.ThrowIfNotDefined(GlobalStringVault.IsDefined(self), self);

#if __ENCOSY_SHARED_STRING_VAULT__
            if (s_managedStringMap.TryGetValue(self, out var managedString) == false)
            {
                managedString = GlobalStringVault.GetString(self).ToString();
                s_managedStringMap.TryAdd(self, managedString);
            }

            return managedString;
#else
            return GlobalStringVault.GetString(self)
#endif
        }

#if __ENCOSY_SHARED_STRING_VAULT__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String ToFixedStringGlobal<T>(this StringId<T> self)
            => ToFixedStringGlobal(self.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String ToFixedStringGlobal(this StringId self)
            => GlobalStringVault.GetString(self);
#endif
    }
}
