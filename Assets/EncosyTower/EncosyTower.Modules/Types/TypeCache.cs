using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules
{
    public static class TypeCache
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type Get<T>()
            => TypeCache<T>.Type;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Is<T>(this Type type)
            => type == TypeCache<T>.Type;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeHash GetHash<T>()
            => TypeCache<T>.Hash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetName<T>()
            => TypeCache<T>.Type.Name;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnmanaged<T>()
            => RuntimeHelpers.IsReferenceOrContainsReferences<T>() == false;

        public static TypeId GetId<T>()
        {
            var id = new TypeId(TypeIdVault.Cache<T>.Id);
            TypeIdVault.Register(id._value, TypeCache<T>.Type);
            return id;
        }
    }

    public static class TypeCache<T>
    {
        private readonly static Type s_type;
        private readonly static bool s_isUnmanaged;

        static TypeCache()
        {
            s_type = typeof(T);
            s_isUnmanaged = RuntimeHelpers.IsReferenceOrContainsReferences<T>() == false;
        }

        public static Type Type
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_type;
        }

        public static bool IsUnmanaged
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_isUnmanaged;
        }

        public static bool IsValueType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_type.IsValueType;
        }

        public static TypeHash Hash
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_type;
        }
    }
}
