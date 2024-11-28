using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace EncosyTower.Modules.Unions.Converters
{
    public static partial class UnionConverter
    {
        private static ConcurrentDictionary<TypeId, IUnionConverter> s_converters;

        static UnionConverter()
        {
            Init();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            s_converters = new();

            TryRegisterGeneratedConverters();
            TryRegister(UnionConverterString.Default);
            TryRegister(UnionConverterObject.Default);
        }

        static partial void TryRegisterGeneratedConverters();

        public static bool TryRegister<T>(IUnionConverter<T> converter)
        {
            ThrowIfNullOrSizeOfTIsBiggerThanUnionDataSize(converter);

            return s_converters.TryAdd((TypeId)TypeId<T>.Value, converter);
        }

        public static IUnionConverter<T> GetConverter<T>()
        {
            if (s_converters.TryGetValue((TypeId)TypeId<T>.Value, out var candidate))
            {
                if (candidate is IUnionConverter<T> converterT)
                {
                    return converterT;
                }
            }

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                return UnionConverterObject<T>.Default;
            }

            return UnionConverterUndefined<T>.Default;
        }

        public static bool TryGetConverter(TypeId typeId, out IUnionConverter result)
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
        public static Union ToUnion<T>(T value)
            => GetConverter<T>().ToUnion(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Union<T> ToUnionT<T>(T value)
            => GetConverter<T>().ToUnionT(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValue<T>(in Union union)
            => GetConverter<T>().GetValue(union);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(in Union union, out T result)
            => GetConverter<T>().TryGetValue(union, out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySetValueTo<T>(in Union union, ref T dest)
            => GetConverter<T>().TrySetValueTo(union, ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToString<T>(in Union union)
            => GetConverter<T>().ToString(union);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToString(in Union union)
        {
            return TryGetConverter(union.TypeId, out var converter)
                ? converter.ToString(union)
                : union.TypeId.ToType().ToString();
        }

        [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfNullOrSizeOfTIsBiggerThanUnionDataSize<T>(IUnionConverter<T> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                return;
            }

            var typeOfT = typeof(T);
            var sizeOfT = UnsafeUtility.SizeOf(typeOfT);

            if (sizeOfT > UnionData.BYTE_COUNT)
            {
                throw new NotSupportedException(
                    $"The size of {typeOfT} is {sizeOfT} bytes, " +
                    $"while a Union can only store {UnionData.BYTE_COUNT} bytes of custom data. " +
                    $"To enable the automatic conversion between {typeOfT} and {typeof(Union)}, " +
                    $"please {GetDefineSymbolMessage(sizeOfT)}"
                );
            }

            static string GetDefineSymbolMessage(int size)
            {
                var longCount = (int)Math.Ceiling((double)size / UnionData.SIZE_OF_LONG);
                var nextSize = longCount * UnionData.SIZE_OF_LONG;

                if (size > UnionData.MAX_BYTE_COUNT)
                {
                    return $"contact the author to increase the maximum size of Union type to {nextSize} bytes " +
                        $"(currently it is {UnionData.MAX_BYTE_COUNT} bytes).";
                }
                else
                {
                    return $"define UNION_{nextSize}_BYTES, or UNION_{longCount * 2}_INTS, or UNION_{longCount}_LONGS.";
                }
            }
        }
    }
}
