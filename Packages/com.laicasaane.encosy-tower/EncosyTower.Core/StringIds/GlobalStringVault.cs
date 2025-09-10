using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.StringIds
{
    internal static class GlobalStringVault
    {
        private readonly static StringVault s_vault = new(256);

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode, UnityEngine.Scripting.Preserve]
        private static void InitWhenDomainReloadDisabled()
        {
            // DO NOT clear the `s_vault`!!!
            // Allocated StringId value stored in static fields should persist upon entering play mode every time.
            // Because when Domain Reload is disabled, the Unity Editor is always in a single session,
            // string.GetHashCode will persist as well.
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefined(StringId key)
            => s_vault.ContainsId(key.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringId MakeIdFromUnmanaged(in UnmanagedString str)
        {
            return new(s_vault.MakeIdFromUnmanaged(str));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringId MakeIdFromManaged([NotNull] string str)
        {
            return new(s_vault.MakeIdFromManaged(str));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnmanagedString GetUnmanagedString(StringId key)
        {
            s_vault.TryGetUnmanagedString(key.Id, out var result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetManagedString(StringId key)
        {
            s_vault.TryGetManagedString(key.Id, out var result);
            return result;
        }

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        internal static void ThrowIfNotDefined([DoesNotReturnIf(false)] bool isDefined, StringId key)
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
