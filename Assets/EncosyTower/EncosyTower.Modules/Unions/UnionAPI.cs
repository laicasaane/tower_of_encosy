using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Unions
{
    public static class UnionAPI
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Union AsUnion<T>(this T value)
            => Union<T>.ToUnion(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Union<T> AsUnionT<T>(this T value)
            => Union<T>.ToUnionT(value);
    }
}
