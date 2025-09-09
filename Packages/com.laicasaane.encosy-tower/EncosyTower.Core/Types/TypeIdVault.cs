using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Types
{
    internal static class TypeIdVault
    {
        #pragma warning disable IDE1006
        // ReSharper disable once InconsistentNaming
        private readonly struct __UndefinedType__ { }
        #pragma warning restore IDE1006

        private static readonly object s_lock = new();
        private static readonly ConcurrentDictionary<TypeId, Type> s_idToTypeVault = new();
        private static readonly ConcurrentDictionary<Type, TypeId> s_typeToIdVault = new();
        private static uint s_current;

        public static readonly Type UndefinedType = typeof(__UndefinedType__);

        private static uint Next
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

        internal static void Init()
        {
            _ = Type<__UndefinedType__>.Id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TypeId Register(Type type)
        {
            TypeId id = new(Next);
            s_idToTypeVault.TryAdd(id, type);
            s_typeToIdVault.TryAdd(type, id);
            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryGetType(TypeId id, out Type type)
            => s_idToTypeVault.TryGetValue(id, out type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryGetId(Type type, out TypeId id)
            => s_typeToIdVault.TryGetValue(type, out id);
    }
}
