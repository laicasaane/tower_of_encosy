using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.StringIds
{
    internal static class GlobalStringVault<T>
    {
        private readonly static StringVault s_vault = new(256);

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
        public static StringId<T> MakeId([NotNull] string str)
            => new(s_vault.MakeId(str), false);

#pragma warning restore CS0618 // Type or member is obsolete

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(StringId<T> key)
            => s_vault.TryGetString(key.Id, out var str) ? str : string.Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefined(StringId<T> key)
            => s_vault.ContainsId(key.Id);

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
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
