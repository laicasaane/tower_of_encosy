#if UNITY_COLLECTIONS
#define __ENCOSY_SHARED_STRING_VAULT__
#endif

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.StringIds
{
#if __ENCOSY_SHARED_STRING_VAULT__
    using String = Unity.Collections.FixedString64Bytes;
#else
    using String = System.String;
#endif

    internal static class GlobalStringVault
    {
        private readonly static StringVault s_vault = new(256);

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            s_vault.Clear();
        }
#endif

        public static int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_vault.Capacity;
        }

        public static int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_vault.Count;
        }

#pragma warning disable CS0618 // Type or member is obsolete

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringId MakeId(
#if __ENCOSY_SHARED_STRING_VAULT__
            in
#else
            [NotNull]
#endif
            String str
        )
        {
            return new(s_vault.MakeId(str));
        }

#pragma warning restore CS0618 // Type or member is obsolete

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String GetString(StringId key)
        {
            return s_vault.TryGetString(key.Id, out var str)
                ? str
#if __ENCOSY_SHARED_STRING_VAULT__
                : default
#else
                : string.Empty
#endif
                ;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefined(StringId key)
            => s_vault.ContainsId(key.Id);

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowIfNotDefined([DoesNotReturnIf(false)] bool isDefined, StringId key)
        {
            if (isDefined == false)
            {
                throw new InvalidOperationException(
                    $"No StringId has not been globally defined with id \"{key}\". " +
                    $"To define one, use StringToId.Get() API."
                );
            }
        }
    }
}
