using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Types
{
    /// <summary>
    /// Provides information about the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Type<T>
    {
        private readonly static Type s_type;
        private readonly static TypeInfo<T> s_info;

        static Type()
        {
            s_type = typeof(T);

            var info = RuntimeTypeCache.Register<T>(s_type);
            s_info = new(info.Id, info.Hash, info.IsValueType, info.IsUnmanaged, info.IsBlittable);
        }

        /// <summary>
        /// Gets the <see cref="TypeId{T}"/> of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static TypeId<T> Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_info.Id;
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
            get => s_info.IsValueType;
        }

        /// <summary>
        /// Determines whether <typeparamref name="T"/> is unmanaged.
        /// </summary>
        public static bool IsUnmanaged
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_info.IsUnmanaged;
        }

        /// <summary>
        /// Determines whether <typeparamref name="T"/> is both unmanaged and blittable.
        /// </summary>
        public static bool IsBlittable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_info.IsBlittable;
        }

        /// <summary>
        /// Gets the hash code of <typeparamref name="T"/>.
        /// </summary>
        public static TypeHash Hash
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_type;
        }

        public static ref readonly TypeInfo<T> Info
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref s_info;
        }
    }
}
