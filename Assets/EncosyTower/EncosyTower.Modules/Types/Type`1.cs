using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules
{
    /// <summary>
    /// Provides information about the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Type<T>
    {
        private readonly static Type s_type;
        private readonly static bool s_isUnmanaged;
        private readonly static bool s_isBlittable;

        static Type()
        {
            s_type = typeof(T);
            s_isUnmanaged = RuntimeHelpers.IsReferenceOrContainsReferences<T>() == false;
            s_isBlittable = s_isUnmanaged && s_type.IsAutoLayout == false && s_type != Type<bool>.s_type;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> of <typeparamref name="T"/>.
        /// </summary>
        public static Type Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_type;
        }

        /// <summary>
        /// Determines whether <typeparamref name="T"/> is a value type.
        /// </summary>
        public static bool IsValueType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_type.IsValueType;
        }

        /// <summary>
        /// Determines whether <typeparamref name="T"/> is unmanaged.
        /// </summary>
        public static bool IsUnmanaged
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_isUnmanaged;
        }

        /// <summary>
        /// Determines whether <typeparamref name="T"/> is both unmanaged and blittable.
        /// </summary>
        public static bool IsBlittable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_isBlittable;
        }

        /// <summary>
        /// Gets the hash code of <typeparamref name="T"/>.
        /// </summary>
        public static TypeHash Hash
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_type;
        }
    }
}
