using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules.Unions.Converters
{
    internal sealed class UnionConverterString : IUnionConverter<string>
    {
        public static readonly UnionConverterString Default = new();

        private UnionConverterString() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(string value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<string> ToUnionT(string value)
            => new Union(value);

        public string GetValue(in Union union)
        {
            if (union.TryGetValue(out string result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out string result)
            => union.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref string dest)
            => union.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Union union)
            => union.Object?.ToString() ?? string.Empty;

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(string)} from the input union.");
        }
    }
}
