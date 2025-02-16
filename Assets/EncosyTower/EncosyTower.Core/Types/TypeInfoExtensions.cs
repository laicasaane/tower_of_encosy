using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.Types
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
