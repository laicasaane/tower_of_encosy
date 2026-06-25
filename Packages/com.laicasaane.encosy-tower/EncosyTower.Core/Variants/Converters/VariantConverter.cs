using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Types;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace EncosyTower.Variants.Converters
{
    public static partial class VariantConverter
    {
        private static ConcurrentDictionary<TypeId, IVariantConverter> s_converters;

        static VariantConverter()
        {
            Init();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode, UnityEngine.Scripting.Preserve]
#endif
        private static void Init()
        {
            s_converters = new();

            TryRegisterGeneratedConverters();
            TryRegister(VariantConverterString.Default);
            TryRegister(VariantConverterObject.Default);
        }

        static partial void TryRegisterGeneratedConverters();

        public static bool TryRegister<T>([NotNull] IVariantConverter<T> converter)
        {
            ThrowIfSizeInvalid<T>(IsSizeValid<T>());

            return s_converters.TryAdd((TypeId)Type<T>.Id, converter);
        }

        public static IVariantConverter<T> GetConverter<T>()
        {
            if (s_converters.TryGetValue((TypeId)Type<T>.Id, out var candidate))
            {
                if (candidate is IVariantConverter<T> converterT)
                {
                    return converterT;
                }
            }

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                return VariantConverterObject<T>.Default;
            }

            return VariantConverterUndefined<T>.Default;
        }

        public static bool TryGetConverter(TypeId typeId, out IVariantConverter result)
        {
            if (s_converters.TryGetValue(typeId, out var candidate) && candidate is not null)
            {
                result = candidate;
                return true;
            }

            result = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Variant ToVariant<T>(T value)
            => GetConverter<T>().ToVariant(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Variant<T> ToVariantT<T>(T value)
            => GetConverter<T>().ToVariantT(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValue<T>(in Variant variant)
            => GetConverter<T>().GetValue(variant);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(in Variant variant, out T result)
            => GetConverter<T>().TryGetValue(variant, out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySetValueTo<T>(in Variant variant, ref T dest)
            => GetConverter<T>().TrySetValueTo(variant, ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToString<T>(in Variant variant)
            => GetConverter<T>().ToString(variant);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToString(in Variant variant)
        {
            return TryGetConverter(variant.TypeId, out var converter)
                ? converter.ToString(variant)
                : variant.TypeId.ToType().ToString();
        }

        private static bool IsSizeValid<T>()
        {
            if (EncosyTypeExtensions.IsUnmanaged<T>() == false)
            {
                return true;
            }

            var sizeOfT = UnsafeUtility.SizeOf(typeof(T));
            return sizeOfT <= VariantData.BYTE_COUNT;
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfSizeInvalid<T>([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateException();
            }

            return;

            [MethodImpl(MethodImplOptions.NoInlining)]
            static NotSupportedException CreateException()
                => new(
                    $"The size of {typeof(T)} is {UnsafeUtility.SizeOf(typeof(T))} bytes, " +
                    $"while a Variant can only store {VariantData.BYTE_COUNT} bytes of custom data. " +
                    $"To enable the automatic conversion between {typeof(T)} and {typeof(Variant)}, " +
                    $"please {GetDefineSymbolMessage(UnsafeUtility.SizeOf(typeof(T)))}"
                );

            static string GetDefineSymbolMessage(int size)
            {
                var longCount = (int)Math.Ceiling((double)size / VariantData.SIZE_OF_LONG);
                var nextSize = longCount * VariantData.SIZE_OF_LONG;

                if (size > VariantData.MAX_BYTE_COUNT)
                {
                    return $"contact the author to increase the maximum size of Variant type to {nextSize} bytes " +
                        $"(currently it is capped at {VariantData.MAX_BYTE_COUNT} bytes).";
                }
                else
                {
                    return $"define VARIANT_{longCount}_LONGS, or use the menu 'Encosy Tower/Project Settings/Variant Type'.";
                }
            }
        }
    }
}
