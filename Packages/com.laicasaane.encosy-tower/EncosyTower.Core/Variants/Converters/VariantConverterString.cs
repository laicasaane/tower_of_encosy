using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Variants.Converters
{
    internal sealed class VariantConverterString : IVariantConverter<string>
    {
        public static readonly VariantConverterString Default = new();

        private VariantConverterString() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant ToVariant(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant<string> ToVariantT(string value)
            => new Variant(value);

        public string GetValue(in Variant variant)
        {
            if (variant.TryGetValue(out string result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Variant variant, out string result)
            => variant.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Variant variant, ref string dest)
            => variant.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Variant variant)
            => variant.Object?.ToString() ?? string.Empty;

        [HideInCallstack, StackTraceHidden, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(string)} from the input variant.");
        }
    }
}
