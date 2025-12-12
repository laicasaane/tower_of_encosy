using System;
using System.Diagnostics;
using UnityEngine;

namespace EncosyTower.Entities.Stats
{
    public sealed class StatVariantTypeException : Exception
    {
        public StatVariantTypeException(string message) : base(message) { }
    }

    public sealed class StatVariantOperatorException : Exception
    {
        public StatVariantOperatorException(string message) : base(message) { }
    }

    public sealed class StatDataTypeException : Exception
    {
        public StatDataTypeException(string message) : base(message) { }
    }

    internal static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowIfMismatchedOperatorTypes(StatVariantType lhs, StatVariantType rhs, string op)
        {
            if (lhs == rhs)
            {
                return;
            }

            throw new StatVariantTypeException(
                $"The operands 'lhs' and 'rhs' of the '{op}' operator are of different types, respectively " +
                $"'{lhs.ToStringFast()}' and '{rhs.ToStringFast()}'."
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowIfMismatchedFunctionTypes(StatVariantType a, StatVariantType b, string func)
        {
            if (a == b)
            {
                return;
            }

            throw new StatVariantTypeException(
                $"The arguments 'a' and 'b' of the '{func}' function are of different types, respectively " +
                $"'{a.ToStringFast()}' and '{b.ToStringFast()}'."
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowIfMismatchedClampTypes(
            StatVariantType value, StatVariantType lowerBound, StatVariantType upperBound
        )
        {
            if (value == lowerBound && lowerBound == upperBound)
            {
                return;
            }

            throw new StatVariantTypeException(
                $"The arguments 'value', 'lowerBound' and 'upperBound' of the 'Clamp' function " +
                $"are of different types, respectively " +
                $"'{value.ToStringFast()}', '{lowerBound.ToStringFast()}', and '{upperBound.ToStringFast()}'."
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowIfExplicitlyConvertToWrongType(StatVariantType type, StatVariantType requiredType)
        {
            if (type == requiredType)
            {
                return;
            }

            throw new StatVariantTypeException(
                $"Cannot explicitly convert a value of type '{type.ToStringFast()}' " +
                $"to type '{requiredType.ToStringFast()}'."
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowIfDestinationTypeMismatch(StatVariantType type, StatVariantType destinationType)
        {
            if (type == destinationType)
            {
                return;
            }

            throw new StatVariantTypeException(
                $"Cannot set a value of type '{type.ToStringFast()}' to type '{destinationType.ToStringFast()}'."
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowIfUnsupportedType(StatVariantType type)
        {
            if (type.IsDefined())
            {
                return;
            }

            throw new StatVariantTypeException($"Value type '{type.ToStringFast()}' is not supported.");
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowUnsupportedType(StatVariantType type)
        {
            throw new StatVariantTypeException($"Value type '{type.ToStringFast()}' is not supported.");
        }

        [HideInCallstack, StackTraceHidden]
        public static StatVariantTypeException UnsupportedTypeException(StatVariantType type)
            => new($"Value type '{type.ToStringFast()}' is not supported.");

        [HideInCallstack, StackTraceHidden]
        public static StatVariantOperatorException OperatorException(string op, StatVariantType type)
            => new($"Cannot apply operator '{op}' to stat value of type '{type.ToStringFast()}'.");

        [HideInCallstack, StackTraceHidden]
        public static StatVariantOperatorException FuncException(string func, StatVariantType type)
            => new($"Cannot use function '{func}' with stat value of type '{type.ToStringFast()}'.");

        [HideInCallstack, StackTraceHidden]
        public static StatVariantOperatorException UnaryOperatorException(string op, StatVariantType type)
            => new($"Cannot apply unary operator '{op}' to stat value of type '{type.ToStringFast()}'.");

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        internal static void ThrowIfStatWorldDataIsNotCreated(bool isCreated)
        {
            if (isCreated)
            {
                return;
            }

            throw new ArgumentException("Stat World data is not created", "worldData");
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        internal static void ThrowIfPairsMismatch<TStatData>(StatVariantType valuePairType, TStatData statData)
            where TStatData : IStatData
        {
            if (valuePairType == statData.ValueType)
            {
                return;
            }

            if (statData.IsValuePair)
            {
                throw new StatDataTypeException(
                    $"Stat data '{typeof(TStatData)}' requires value of type '{statData.ValueType.ToStringFast()}' " +
                    $"but receives a ValuePair of type '{valuePairType.ToStringFast()}'."
                );
            }
        }
    }
}
