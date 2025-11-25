using System;
using System.Diagnostics;
using UnityEngine;

namespace EncosyTower.Entities.Stats
{
    public sealed class StatValueTypeException : Exception
    {
        public StatValueTypeException(string message) : base(message) { }
    }

    public sealed class StatValueOperatorException : Exception
    {
        public StatValueOperatorException(string message) : base(message) { }
    }

    internal static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowIfMismatchedTypes(StatVariantType baseType, StatVariantType currentType)
        {
            if (baseType == currentType)
            {
                return;
            }

            throw new StatValueTypeException(
                $"Base value and current value are of different types, respectively '{baseType}' and '{currentType}'."
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowIfExplicitlyConvertToWrongType(StatVariantType type, StatVariantType requiredType)
        {
            if (type == requiredType)
            {
                return;
            }

            throw new StatValueTypeException(
                $"Cannot explicitly convert a value of type '{type}' to type '{requiredType}'."
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowIfDestinationTypeMismatch(StatVariantType type, StatVariantType destinationType)
        {
            if (type == destinationType)
            {
                return;
            }

            throw new StatValueTypeException(
                $"Cannot set a value of type '{type}' to type '{destinationType}'."
            );
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowIfUnsupportedType(StatVariantType type)
        {
            if (type.IsDefined())
            {
                return;
            }

            throw new StatValueTypeException($"Value type '{type}' is not supported.");
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void ThrowUnsupportedType(StatVariantType type)
        {
            throw new StatValueTypeException($"Value type '{type}' is not supported.");
        }

        [HideInCallstack, StackTraceHidden]
        public static StatValueTypeException UnsupportedTypeException(StatVariantType type)
            => new($"Value type '{type}' is not supported.");

        [HideInCallstack, StackTraceHidden]
        public static StatValueOperatorException OperatorException(string op, StatVariantType type)
            => new($"Cannot apply operator '{op}' to stat value of type '{type}'.");

        [HideInCallstack, StackTraceHidden]
        public static StatValueOperatorException FuncException(string func, StatVariantType type)
            => new($"Cannot use function '{func}' with stat value of type '{type}'.");

        [HideInCallstack, StackTraceHidden]
        public static StatValueOperatorException UnaryOperatorException(string op, StatVariantType type)
            => new($"Cannot apply unary operator '{op}' to stat value of type '{type}'.");

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        internal static void ThrowIfStatWorldDataIsNotCreated(bool isCreated)
        {
            if (isCreated)
            {
                return;
            }

            throw new ArgumentException("Stat World data is not created", "worldData");
        }
    }
}
