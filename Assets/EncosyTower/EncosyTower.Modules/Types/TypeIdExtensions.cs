using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Types
{
    public static class TypeIdExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type ToType(this TypeId self)
        {
            return TypeIdVault.TryGetType(self, out var type)
                ? type : TypeIdVault.UndefinedType;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryToType(this TypeId self, out Type type)
            => TypeIdVault.TryGetType(self, out type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type ToType<T>(this TypeId<T> _)
            => Type<T>.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsType<T>(this TypeId self)
            => TypeIdVault.TryGetType(self, out var type) && type == Type<T>.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<TypeId<T>> AsTyped<T>(this TypeId self)
            => self.IsType<T>() ? Type<T>.Id : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Id2 ToId2(this TypeId self)
            => new((Id)self, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Id2 ToId2(this TypeId self, Id id)
            => new((Id)self, id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Id2 ToId2<T>(this TypeId<T> self)
            => new((Id<T>)self, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Id2 ToId2<T>(this TypeId<T> self, Id id)
            => new((Id<T>)self, id);
    }
}
