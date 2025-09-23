using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Variants.Converters
{
    internal sealed class VariantConverterObject : IVariantConverter<object>
    {
        public static readonly VariantConverterObject Default = new();

        private VariantConverterObject() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant ToVariant(object value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant<object> ToVariantT(object value)
            => new Variant(value);

        public object GetValue(in Variant variant)
        {
            if (variant.TryGetValue(out object result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Variant variant, out object result)
            => variant.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Variant variant, ref object dest)
            => variant.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Variant variant)
            => variant.Object?.ToString() ?? string.Empty;

        [HideInCallstack, StackTraceHidden, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(object)} from the input variant.");
        }
    }
}
