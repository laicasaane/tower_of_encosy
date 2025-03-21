/// <auto-generated>
///*********************************************************///
///                                                         ///
/// This file is auto-generated by UnionConvertersGenerator ///
///               DO NOT manually modify it!                ///
///                                                         ///
///*********************************************************///
/// </auto-generated>

#pragma warning disable

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Unions.Converters
{
    partial class UnionConverter
    {
        static partial void TryRegisterGeneratedConverters()
        {
            TryRegister(UnionConverterBool.Default);
            TryRegister(UnionConverterByte.Default);
            TryRegister(UnionConverterSByte.Default);
            TryRegister(UnionConverterChar.Default);
            TryRegister(UnionConverterDouble.Default);
            TryRegister(UnionConverterFloat.Default);
            TryRegister(UnionConverterInt.Default);
            TryRegister(UnionConverterUInt.Default);
            TryRegister(UnionConverterLong.Default);
            TryRegister(UnionConverterULong.Default);
            TryRegister(UnionConverterShort.Default);
            TryRegister(UnionConverterUShort.Default);
        }
    }

    internal sealed class UnionConverterBool : IUnionConverter<bool>
    {
        public static readonly UnionConverterBool Default = new UnionConverterBool();

        private UnionConverterBool() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(bool value) => new Union(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<bool> ToUnionT(bool value) => new Union(value);

        public bool GetValue(in Union union)
        {
            if (union.TryGetValue(out bool result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out bool result) => union.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref bool dest) => union.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Union union) => union.Bool.ToString();

        [HideInCallstack, DoesNotReturn]
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(bool)} from the input union.");
        }

    }

    internal sealed class UnionConverterByte : IUnionConverter<byte>
    {
        public static readonly UnionConverterByte Default = new UnionConverterByte();

        private UnionConverterByte() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(byte value) => new Union(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<byte> ToUnionT(byte value) => new Union(value);

        public byte GetValue(in Union union)
        {
            if (union.TryGetValue(out byte result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out byte result) => union.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref byte dest) => union.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Union union) => union.Byte.ToString();

        [HideInCallstack, DoesNotReturn]
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(byte)} from the input union.");
        }

    }

    internal sealed class UnionConverterSByte : IUnionConverter<sbyte>
    {
        public static readonly UnionConverterSByte Default = new UnionConverterSByte();

        private UnionConverterSByte() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(sbyte value) => new Union(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<sbyte> ToUnionT(sbyte value) => new Union(value);

        public sbyte GetValue(in Union union)
        {
            if (union.TryGetValue(out sbyte result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out sbyte result) => union.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref sbyte dest) => union.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Union union) => union.SByte.ToString();

        [HideInCallstack, DoesNotReturn]
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(sbyte)} from the input union.");
        }

    }

    internal sealed class UnionConverterChar : IUnionConverter<char>
    {
        public static readonly UnionConverterChar Default = new UnionConverterChar();

        private UnionConverterChar() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(char value) => new Union(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<char> ToUnionT(char value) => new Union(value);

        public char GetValue(in Union union)
        {
            if (union.TryGetValue(out char result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out char result) => union.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref char dest) => union.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Union union) => union.Char.ToString();

        [HideInCallstack, DoesNotReturn]
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(char)} from the input union.");
        }

    }

    internal sealed class UnionConverterDouble : IUnionConverter<double>
    {
        public static readonly UnionConverterDouble Default = new UnionConverterDouble();

        private UnionConverterDouble() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(double value) => new Union(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<double> ToUnionT(double value) => new Union(value);

        public double GetValue(in Union union)
        {
            if (union.TryGetValue(out double result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out double result) => union.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref double dest) => union.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Union union) => union.Double.ToString();

        [HideInCallstack, DoesNotReturn]
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(double)} from the input union.");
        }

    }

    internal sealed class UnionConverterFloat : IUnionConverter<float>
    {
        public static readonly UnionConverterFloat Default = new UnionConverterFloat();

        private UnionConverterFloat() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(float value) => new Union(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<float> ToUnionT(float value) => new Union(value);

        public float GetValue(in Union union)
        {
            if (union.TryGetValue(out float result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out float result) => union.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref float dest) => union.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Union union) => union.Float.ToString();

        [HideInCallstack, DoesNotReturn]
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(float)} from the input union.");
        }

    }

    internal sealed class UnionConverterInt : IUnionConverter<int>
    {
        public static readonly UnionConverterInt Default = new UnionConverterInt();

        private UnionConverterInt() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(int value) => new Union(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<int> ToUnionT(int value) => new Union(value);

        public int GetValue(in Union union)
        {
            if (union.TryGetValue(out int result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out int result) => union.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref int dest) => union.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Union union) => union.Int.ToString();

        [HideInCallstack, DoesNotReturn]
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(int)} from the input union.");
        }

    }

    internal sealed class UnionConverterUInt : IUnionConverter<uint>
    {
        public static readonly UnionConverterUInt Default = new UnionConverterUInt();

        private UnionConverterUInt() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(uint value) => new Union(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<uint> ToUnionT(uint value) => new Union(value);

        public uint GetValue(in Union union)
        {
            if (union.TryGetValue(out uint result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out uint result) => union.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref uint dest) => union.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Union union) => union.UInt.ToString();

        [HideInCallstack, DoesNotReturn]
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(uint)} from the input union.");
        }

    }

    internal sealed class UnionConverterLong : IUnionConverter<long>
    {
        public static readonly UnionConverterLong Default = new UnionConverterLong();

        private UnionConverterLong() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(long value) => new Union(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<long> ToUnionT(long value) => new Union(value);

        public long GetValue(in Union union)
        {
            if (union.TryGetValue(out long result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out long result) => union.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref long dest) => union.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Union union) => union.Long.ToString();

        [HideInCallstack, DoesNotReturn]
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(long)} from the input union.");
        }

    }

    internal sealed class UnionConverterULong : IUnionConverter<ulong>
    {
        public static readonly UnionConverterULong Default = new UnionConverterULong();

        private UnionConverterULong() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(ulong value) => new Union(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<ulong> ToUnionT(ulong value) => new Union(value);

        public ulong GetValue(in Union union)
        {
            if (union.TryGetValue(out ulong result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out ulong result) => union.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref ulong dest) => union.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Union union) => union.ULong.ToString();

        [HideInCallstack, DoesNotReturn]
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(ulong)} from the input union.");
        }

    }

    internal sealed class UnionConverterShort : IUnionConverter<short>
    {
        public static readonly UnionConverterShort Default = new UnionConverterShort();

        private UnionConverterShort() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(short value) => new Union(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<short> ToUnionT(short value) => new Union(value);

        public short GetValue(in Union union)
        {
            if (union.TryGetValue(out short result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out short result) => union.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref short dest) => union.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Union union) => union.Short.ToString();

        [HideInCallstack, DoesNotReturn]
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(short)} from the input union.");
        }

    }

    internal sealed class UnionConverterUShort : IUnionConverter<ushort>
    {
        public static readonly UnionConverterUShort Default = new UnionConverterUShort();

        private UnionConverterUShort() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union ToUnion(ushort value) => new Union(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Union<ushort> ToUnionT(ushort value) => new Union(value);

        public ushort GetValue(in Union union)
        {
            if (union.TryGetValue(out ushort result) == false)
            {
                ThrowIfInvalidCast();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Union union, out ushort result) => union.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Union union, ref ushort dest) => union.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Union union) => union.UShort.ToString();

        [HideInCallstack, DoesNotReturn]
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCast()
        {
            throw new InvalidCastException($"Cannot get value of {typeof(ushort)} from the input union.");
        }

    }

}

