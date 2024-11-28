using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules.Unions.Converters
{
    internal sealed class UnionConverterObject<T> : IUnionConverter<T>
    {
        public static readonly UnionConverterObject<T> Default = new();

        private UnionConverterObject() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(T value)
            => new((TypeId)TypeId<T>.Value, (object)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<T> ToUnionT(T value)
            => new Union((TypeId)TypeId<T>.Value, (object)value);

        public T GetValue(in Union union)
        {
            if (union.TryGetValue(out object candidate)
                && candidate is T value
            )
            {
                return value;
            }

            ThrowIfInvalidCast();
            return default;
        }

        public bool TryGetValue(in Union union, out T result)
        {
            if (union.TryGetValue(out object candidate) && candidate is T value)
            {
                result = value;
                return true;
            }

            result = default;
            return false;
        }

        public bool TrySetValueTo(in Union union, ref T dest)
        {
            if (union.TryGetValue(out object candidate) && candidate is T value)
            {
                dest = value;
                return true;
            }

            return false;
        }

        public string ToString(in Union union)
        {
            if (union.TryGetValue(out object candidate) && candidate is T value)
            {
                return value.ToString();
            }

            return union.TypeId.ToType().ToString() ?? string.Empty;
        }

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(T)} from the input union.");
        }
    }
}
