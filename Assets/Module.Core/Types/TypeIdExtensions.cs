using System.Runtime.CompilerServices;

namespace Module.Core
{
    public static class TypeIdExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOfType<T>(this TypeId self)
            => self.ToType() == typeof(T);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Id2 ToId2(this TypeId self)
            => new((Id)self, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Id2 ToId2(this TypeId self, Id id)
            => new((Id)self, id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Id2 ToId2<T>(this TypeId<T> self)
            => new((Id)(Id<T>)self, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Id2 ToId2<T>(this TypeId<T> self, Id id)
            => new((Id)(Id<T>)self, id);
    }
}
