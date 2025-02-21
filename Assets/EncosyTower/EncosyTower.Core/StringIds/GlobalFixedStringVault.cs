#if UNITY_BURST && UNITY_COLLECTIONS

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Ids;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace EncosyTower.StringIds
{
    /// <summary>
    /// The vault to store instances of <see cref="FixedString32Bytes"/>.
    /// </summary>
    /// <remarks>
    /// Due to technical limitations of <see cref="SharedStatic{T}"/>
    /// the vault can only store up to 256 unique strings.
    /// </remarks>
    /// <seealso cref="FixedStringVault"/>
    internal static class GlobalFixedStringVault<T>
    {
        public const int MAX_CAPACITY = FixedStringVault.MAX_CAPACITY;

        private readonly static SharedStatic<FixedStringVault> s_vault
            = SharedStatic<FixedStringVault>.GetOrCreate<FixedStringVault, T>();

        static GlobalFixedStringVault()
        {
            s_vault.Data = new FixedStringVault();
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
        public static StringId<T> MakeId(string str)
        {
            var fixedString = new FixedString32Bytes();
            fixedString.Append(str);
            return MakeId(fixedString);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringId<T> MakeId(in FixedString32Bytes str)
        {
            ref var vault = ref s_vault.Data;
            return new(vault.MakeId(str), true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes GetString(StringId<T> key)
            => s_vault.Data.TryGetString(key.Id, out var str) ? str : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefined(StringId<T> key)
            => s_vault.Data.ContainsId(key.Id);

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowIfNotDefined([DoesNotReturnIf(false)] bool isDefined, StringId<T> key)
        {
            if (isDefined == false)
            {
                throw new InvalidOperationException(
                    $"No StringId<{typeof(T).Name}> has not been globally defined with id \"{key}\". " +
                    $"To define one, use StringToId.Get() API."
                );
            }
        }
    }
}

#endif
