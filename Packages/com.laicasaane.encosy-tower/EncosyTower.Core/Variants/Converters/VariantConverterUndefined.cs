using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Types;
using UnityEngine;

namespace EncosyTower.Variants.Converters
{
    internal sealed class VariantConverterUndefined<T> : IVariantConverter<T>
    {
        public static readonly VariantConverterUndefined<T> Default = new();

        private VariantConverterUndefined() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant ToVariant(T value)
            => new(VariantTypeKind.Undefined, (TypeId)Type<T>.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant<T> ToVariantT(T value)
            => new Variant(VariantTypeKind.Undefined, (TypeId)Type<T>.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue(in Variant variant)
        {
            ThrowIfInvalidCast();
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Variant variant, out T result)
        {
            result = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Variant variant, ref T dest)
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Variant variant)
        {
            return variant.ToString();
        }

        [HideInCallstack, StackTraceHidden, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(T)} from the input variant.");
        }
    }
}
