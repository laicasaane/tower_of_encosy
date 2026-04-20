using System.Runtime.CompilerServices;

namespace EncosyTower.Common
{
    public static class GenericT
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T<TType> T<TType>()
            => default;
    }

    public readonly struct T<TType> { }
}
