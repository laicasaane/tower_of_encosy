using System.Runtime.CompilerServices;

namespace EncosyTower.Modules
{
    /// <summary>
    /// Provides information about the type of <typeparamref name="T"/>.
    /// </summary>
    public readonly record struct TypeInfo<T>
    {
        /// <summary>
        /// The unique identifier of the type.
        /// </summary>
        public readonly TypeId<T> Id;

        /// <summary>
        /// The hash code of the type.
        /// </summary>
        public readonly TypeHash Hash;

        /// <summary>
        /// Determines whether the type is a value type.
        /// </summary>
        public readonly ByteBool IsValueType;

        /// <summary>
        /// Determines whether the type is unmanaged.
        /// </summary>
        public readonly ByteBool IsUnmanaged;

        /// <summary>
        /// Determines whether the type is both unmanaged and blittable.
        /// </summary>
        public readonly ByteBool IsBlittable;

        internal TypeInfo(TypeId<T> id, TypeHash hash, ByteBool isValueType, ByteBool isUnmanaged, ByteBool isBlittable)
        {
            Id = id;
            Hash = hash;
            IsValueType = isValueType;
            IsUnmanaged = isUnmanaged;
            IsBlittable = isBlittable;
        }

        /// <summary>
        /// Determines whether the type info is valid.
        /// </summary>
        /// <remarks>
        /// A type info is valid if <see cref="Hash"/> is not <see cref="TypeHash.Null"/>.
        /// </remarks>
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Hash != TypeHash.Null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator TypeInfo(in TypeInfo<T> info)
            => new((TypeId)info.Id, info.Hash, info.IsValueType, info.IsUnmanaged, info.IsBlittable);
    }
}
