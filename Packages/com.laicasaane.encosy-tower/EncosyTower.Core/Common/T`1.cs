using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Types;
using UnityEngine;

namespace EncosyTower.Common
{
    public static class GenericT
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T<TType> T<TType>()
        {
            ThrowIfUnsupported<TType>();
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
            ThrowIfUnsupported<TDest>();

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

        [MethodImpl(MethodImplOptions.NoInlining), HideInCallstack, StackTraceHidden]
        internal static void ThrowIfUnsupported<T>()
        {
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(T<>))
            {
                throw new NotSupportedException(
                    $"Nested T<> is not supported." +
                    $"Type 'T<{typeof(T).GetFriendlyName()}>' cannot be wrapped in another T<>."
                );
            }
        }
    }

    public readonly struct T<TType>
    {
        public T()
        {
            GenericT.ThrowIfUnsupported<TType>();
        }
    }
}
