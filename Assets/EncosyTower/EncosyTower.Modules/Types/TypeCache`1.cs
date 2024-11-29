using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules
{
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
