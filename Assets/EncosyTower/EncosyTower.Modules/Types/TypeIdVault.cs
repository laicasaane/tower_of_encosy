using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace EncosyTower.Modules
{
    internal static class TypeIdVault
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Clearly denotes an undefined type")]
        private readonly struct __UndefinedType__ { }

        private static readonly object s_lock = new();
        private static readonly ConcurrentDictionary<uint, Type> s_vault = new();
        private static uint s_current;

        static TypeIdVault()
        {
            Init();
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        private static void Init()
        {
            s_vault.Clear();
            s_vault.TryAdd(TypeId.Undefined._value, UndefinedType);

            _ = TypeId<bool>.Value;
            _ = TypeId<byte>.Value;
            _ = TypeId<sbyte>.Value;
            _ = TypeId<char>.Value;
            _ = TypeId<decimal>.Value;
            _ = TypeId<double>.Value;
            _ = TypeId<float>.Value;
            _ = TypeId<int>.Value;
            _ = TypeId<uint>.Value;
            _ = TypeId<long>.Value;
            _ = TypeId<ulong>.Value;
            _ = TypeId<short>.Value;
            _ = TypeId<ushort>.Value;
            _ = TypeId<string>.Value;
            _ = TypeId<object>.Value;
        }

        public static readonly Type UndefinedType = typeof(__UndefinedType__);

        internal static uint Next
        {
            get
            {
                lock (s_lock)
                {
                    Interlocked.Add(ref UnsafeUtility.As<uint, int>(ref s_current), 1);
                    return s_current;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Register(uint id, Type type)
            => s_vault.TryAdd(id, type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryGetType(uint id, out Type type)
            => s_vault.TryGetValue(id, out type);

        internal static class Cache<T>
        {
            private static readonly uint s_id;

            public static uint Id
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => s_id;
            }

            static Cache()
            {
#pragma warning disable IDE0002

                s_id = TypeIdVault.Next;
                _ = TypeCache<T>.Type;

#pragma warning restore
#if UNITY_EDITOR && TYPE_ID_DEBUG_LOG
                EncosyTower.Modules.Logging.DevLoggerAPI.LogInfo(
                    $"{nameof(TypeId)} {s_id} is assigned to {typeof(T)}.\n" +
                    $"If the value is overflowed, enabling Domain Reloading will reset it."
                );
#endif
            }
        }
    }
}
