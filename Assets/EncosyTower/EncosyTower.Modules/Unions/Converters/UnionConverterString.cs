using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Unions.Converters
{
    internal sealed class UnionConverterString : IUnionConverter<string>
    {
        public static readonly UnionConverterString Default = new UnionConverterString();

        private UnionConverterString() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(string value)
            => new Union(value);

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

        [DoesNotReturn]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(string)} from the input union.");
        }
    }
}
