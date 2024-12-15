using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Types
{
    public static class TypeInfoExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsType<T>(this TypeInfo self)
            => self.Id.IsType<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<TypeInfo<T>> AsTyped<T>(this TypeInfo self)
            => self.IsType<T>() ? Type<T>.Info : default;
    }
}
