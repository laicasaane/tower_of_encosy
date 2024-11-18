using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules.NameKeys
{
    internal static class GlobalNameVault<T>
    {
        private readonly static NameVault s_vault = new(256);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NameKey<T> NameToKey(string name)
            => new(s_vault.NameToId(name), false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string KeyToName(NameKey<T> key)
            => s_vault.TryGetName(key.Id, out var name) ? name : string.Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefined(NameKey<T> key)
            => s_vault.ContainsId(key.Id);

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowIfNotDefined([DoesNotReturnIf(false)] bool isDefined, NameKey<T> key)
        {
            if (isDefined == false)
            {
                throw new InvalidOperationException(
                    $"No NameKey<{typeof(T).Name}> has not been globally defined with id \"{key}\". " +
                    $"To define one, use NameToKey.Get() API."
                );
            }
        }
    }
}
