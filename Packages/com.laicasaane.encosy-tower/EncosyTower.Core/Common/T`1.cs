using System.Runtime.CompilerServices;

namespace EncosyTower.Common
{
    public static class GenericT
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T<TType> T<TType>()
        {
            return new();
        }

        /// <summary>
        /// Returns an <see cref="Option{TDest}"/> if <paramref name="source"/>
        /// is successfully casted to <typeparamref name="TDest"/>;
        /// otherwise, returns <see cref="Option{TDest}.None"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<TDest> TryCastTo<TSource, TDest>(this TSource source, T<TDest> _)
        {
            if (source is TDest dest)
            {
                return Option.Some(dest);
            }

            return Option<TDest>.None;
        }

        /// <summary>
        /// Returns an <see cref="Option{TDest}"/> if <paramref name="source"/>
        /// is successfully casted to <typeparamref name="TDest"/>;
        /// otherwise, returns <see cref="Option{TDest}.None"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<TDest> TryCastFrom<TSource, TDest>(this T<TDest> _, TSource source)
            => TryCastTo(source, _);
    }

    /// <summary>
    /// Represents a generic version of <c>typeof(TType)</c> which can participates in generic contexts.
    /// </summary>
    public readonly struct T<TType> { }
}
