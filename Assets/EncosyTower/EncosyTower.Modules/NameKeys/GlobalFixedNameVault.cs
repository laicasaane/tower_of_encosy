#if UNITY_BURST && UNITY_COLLECTIONS

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace EncosyTower.Modules.NameKeys
{
    /// <summary>
    /// For each type T, there are maximum 256 unique names.
    /// </summary>
    internal static class GlobalFixedNameVault<T>
    {
        public const int MAX_CAPACITY = FixedNameVault.MAX_CAPACITY;

        private readonly static SharedStatic<FixedNameVault> s_vault
            = SharedStatic<FixedNameVault>.GetOrCreate<FixedNameVault, T>();

        static GlobalFixedNameVault()
        {
            s_vault.Data = new FixedNameVault();
        }

        public static int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_vault.Data.Capacity;
        }

        public static int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_vault.Data.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NameKey<T> NameToKey(string name)
        {
            var fixedName = new FixedString32Bytes();
            fixedName.Append(name);
            return NameToKey(fixedName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NameKey<T> NameToKey(in FixedString32Bytes name)
        {
            ref var vault = ref s_vault.Data;
            return new(vault.NameToId(name), true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes KeyToName(NameKey<T> key)
            => s_vault.Data.TryGetName(key.Id, out var name) ? name : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefined(NameKey<T> key)
            => s_vault.Data.ContainsId(key.Id);

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
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

#endif
