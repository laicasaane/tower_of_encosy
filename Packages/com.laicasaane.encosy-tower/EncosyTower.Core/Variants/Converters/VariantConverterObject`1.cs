using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Types;
using UnityEngine;

namespace EncosyTower.Variants.Converters
{
    internal sealed class VariantConverterObject<T> : IVariantConverter<T>
    {
        public static readonly VariantConverterObject<T> Default = new();

        private VariantConverterObject() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant ToVariant(T value)
            => new((TypeId)Type<T>.Id, (object)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant<T> ToVariantT(T value)
            => new Variant((TypeId)Type<T>.Id, (object)value);

        public T GetValue(in Variant variant)
        {
            var validCast = variant.TryGetValue(out object candidate);
            ThrowIfInvalidCast(validCast & candidate is T);

            return candidate is T value ? value : default;
        }

        public bool TryGetValue(in Variant variant, out T result)
        {
            if (variant.TryGetValue(out object candidate) && candidate is T value)
            {
                result = value;
                return true;
            }

            result = default;
            return false;
        }

        public bool TrySetValueTo(in Variant variant, ref T dest)
        {
            if (variant.TryGetValue(out object candidate) && candidate is T value)
            {
                dest = value;
                return true;
            }

            return false;
        }

        public string ToString(in Variant variant)
        {
            if (variant.TryGetValue(out object candidate) && candidate is T value)
            {
                return value.ToString();
            }

            return variant.TypeId.ToType().ToString() ?? string.Empty;
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidCastException CreateException()
                => new("Cannot get value of object from the input variant.");
        }
    }
}
