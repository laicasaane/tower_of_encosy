using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Module.Core
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

            TypeId.Get<bool>();
            TypeId.Get<byte>();
            TypeId.Get<sbyte>();
            TypeId.Get<char>();
            TypeId.Get<decimal>();
            TypeId.Get<double>();
            TypeId.Get<float>();
            TypeId.Get<int>();
            TypeId.Get<uint>();
            TypeId.Get<long>();
            TypeId.Get<ulong>();
            TypeId.Get<short>();
            TypeId.Get<ushort>();
            TypeId.Get<string>();
            TypeId.Get<object>();
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
                Module.Core.Logging.DevLoggerAPI.LogInfo(
                    $"{nameof(TypeId)} {s_id} is assigned to {typeof(T)}.\n" +
                    $"If the value is overflowed, enabling Domain Reloading will reset it."
                );
#endif
            }
        }
    }
}
