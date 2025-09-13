using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Types;
using EncosyTower.Variants.Converters;

namespace EncosyTower.Variants
{
    /// <summary>
    /// The variant data structure provides a layout and mechanism
    /// to store several types within the same memory position.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    /// <item>The first 8-bytes block store the metadata.</item>
    /// <item>The second 8-byte block stores object reference.</item>
    /// <item>
    /// The rest stores other data.
    /// <br/>
    /// NOTE: By default, the Variant can store any other data whose native size
    /// is lesser or equal to 16 bytes. To increase this capacity,
    /// follow the instruction at <see cref="VariantData"/>.
    /// </item>
    /// </list>
    /// </remarks>
    /// <seealso cref="VariantBase" />
    [StructLayout(LayoutKind.Explicit)]
    public readonly partial struct Variant
    {
        public const int VARIANT_TYPE_KIND_SIZE = sizeof(VariantTypeKind);
        public const int VARIANT_TYPE_ID_OFFSET = VariantBase.META_OFFSET + VARIANT_TYPE_KIND_SIZE;

        public static readonly Variant Undefined = default;

        [FieldOffset(VariantBase.META_OFFSET)] public readonly VariantBase Base;
        [FieldOffset(VariantBase.META_OFFSET)] public readonly VariantTypeKind TypeKind;
        [FieldOffset(VARIANT_TYPE_ID_OFFSET)]  public readonly TypeId TypeId;
        [FieldOffset(VariantBase.OBJECT_OFFSET)] public readonly object Object;
        [FieldOffset(VariantBase.DATA_OFFSET)] public readonly bool Bool;
        [FieldOffset(VariantBase.DATA_OFFSET)] public readonly byte Byte;
        [FieldOffset(VariantBase.DATA_OFFSET)] public readonly sbyte SByte;
        [FieldOffset(VariantBase.DATA_OFFSET)] public readonly char Char;
        [FieldOffset(VariantBase.DATA_OFFSET)] public readonly double Double;
        [FieldOffset(VariantBase.DATA_OFFSET)] public readonly float Float;
        [FieldOffset(VariantBase.DATA_OFFSET)] public readonly int Int;
        [FieldOffset(VariantBase.DATA_OFFSET)] public readonly uint UInt;
        [FieldOffset(VariantBase.DATA_OFFSET)] public readonly long Long;
        [FieldOffset(VariantBase.DATA_OFFSET)] public readonly ulong ULong;
        [FieldOffset(VariantBase.DATA_OFFSET)] public readonly short Short;
        [FieldOffset(VariantBase.DATA_OFFSET)] public readonly ushort UShort;

        public Variant(VariantBase @base, VariantTypeKind type, TypeId typeId) : this()
        {
            Base = @base;
            TypeKind = type;
            TypeId = typeId;
        }

        public Variant(VariantTypeKind type, TypeId typeId) : this()
        {
            TypeKind = type;
            TypeId = typeId;
        }

        public Variant(VariantBase @base) : this()
        {
            Base = @base;
        }

        public Variant(bool value) : this() { TypeKind = VariantTypeKind.Bool; TypeId = (TypeId)Type<bool>.Id; Bool = value; }
        public Variant(byte value) : this() { TypeKind = VariantTypeKind.Byte; TypeId = (TypeId)Type<byte>.Id; Byte = value; }
        public Variant(sbyte value) : this() { TypeKind = VariantTypeKind.SByte; TypeId = (TypeId)Type<sbyte>.Id; SByte = value; }
        public Variant(char value) : this() { TypeKind = VariantTypeKind.Char; TypeId = (TypeId)Type<char>.Id; Char = value; }
        public Variant(double value) : this() { TypeKind = VariantTypeKind.Double; TypeId = (TypeId)Type<double>.Id; Double = value; }
        public Variant(float value) : this() { TypeKind = VariantTypeKind.Float; TypeId = (TypeId)Type<float>.Id; Float = value; }
        public Variant(int value) : this() { TypeKind = VariantTypeKind.Int; TypeId = (TypeId)Type<int>.Id; Int = value; }
        public Variant(uint value) : this() { TypeKind = VariantTypeKind.UInt; TypeId = (TypeId)Type<uint>.Id; UInt = value; }
        public Variant(long value) : this() { TypeKind = VariantTypeKind.Long; TypeId = (TypeId)Type<long>.Id; Long = value; }
        public Variant(ulong value) : this() { TypeKind = VariantTypeKind.ULong; TypeId = (TypeId)Type<ulong>.Id; ULong = value; }
        public Variant(short value) : this() { TypeKind = VariantTypeKind.Short; TypeId = (TypeId)Type<short>.Id; Short = value; }
        public Variant(ushort value) : this() { TypeKind = VariantTypeKind.UShort; TypeId = (TypeId)Type<ushort>.Id; UShort = value; }
        public Variant(string value) : this() { TypeKind = VariantTypeKind.String; TypeId = (TypeId)Type<string>.Id; Object = value; }
        public Variant(object value) : this() { TypeKind = VariantTypeKind.Object; TypeId = (TypeId)Type<object>.Id; Object = value; }

        public Variant(TypeId typeId, object value) : this()
        {
            TypeKind = VariantTypeKind.Object;
            TypeId = typeId;
            Object = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator Variant(bool value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator Variant(byte value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator Variant(sbyte value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator Variant(char value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator Variant(decimal value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator Variant(double value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator Variant(float value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator Variant(int value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator Variant(uint value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator Variant(long value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator Variant(ulong value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator Variant(short value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator Variant(ushort value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator Variant(string value) => new(value);

        public bool TypeEquals(in Variant other)
            => TypeKind == other.TypeKind;

        public bool TryGetValue(out bool result)
        {
            if (TypeKind == VariantTypeKind.Bool)
            {
                result = Bool; return true;
            }

            result = default; return false;
        }

        public bool TryGetValue(out byte result)
        {
            if (TypeKind == VariantTypeKind.Byte)
            {
                result = Byte; return true;
            }

            result = default; return false;
        }

        public bool TryGetValue(out sbyte result)
        {
            if (TypeKind == VariantTypeKind.SByte)
            {
                result = SByte; return true;
            }

            result = default; return false;
        }

        public bool TryGetValue(out char result)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.SByte: result = (char)SByte; return true;
                case VariantTypeKind.Char: result = Char; return true;
                case VariantTypeKind.UShort: result = (char)UShort; return true;
            }

            result = default; return false;
        }

        public bool TryGetValue(out double result)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: result = Byte; return true;
                case VariantTypeKind.SByte: result = SByte; return true;
                case VariantTypeKind.Char: result = Char; return true;
                case VariantTypeKind.Double: result = Double; return true;
                case VariantTypeKind.Float: result = Float; return true;
                case VariantTypeKind.Int: result = Int; return true;
                case VariantTypeKind.UInt: result = UInt; return true;
                case VariantTypeKind.Long: result = Long; return true;
                case VariantTypeKind.ULong: result = ULong; return true;
                case VariantTypeKind.Short: result = Short; return true;
                case VariantTypeKind.UShort: result = UShort; return true;
            }

            result = default; return false;
        }

        public bool TryGetValue(out float result)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: result = Byte; return true;
                case VariantTypeKind.SByte: result = SByte; return true;
                case VariantTypeKind.Char: result = Char; return true;
                case VariantTypeKind.Float: result = Float; return true;
                case VariantTypeKind.Int: result = Int; return true;
                case VariantTypeKind.UInt: result = UInt; return true;
                case VariantTypeKind.Long: result = Long; return true;
                case VariantTypeKind.ULong: result = ULong; return true;
                case VariantTypeKind.Short: result = Short; return true;
                case VariantTypeKind.UShort: result = UShort; return true;
            }

            result = default; return false;
        }

        public bool TryGetValue(out int result)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: result = Byte; return true;
                case VariantTypeKind.SByte: result = SByte; return true;
                case VariantTypeKind.Char: result = Char; return true;
                case VariantTypeKind.Int: result = Int; return true;
                case VariantTypeKind.Short: result = Short; return true;
                case VariantTypeKind.UShort: result = UShort; return true;
            }

            result = default; return false;
        }

        public bool TryGetValue(out uint result)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: result = Byte; return true;
                case VariantTypeKind.Char: result = Char; return true;
                case VariantTypeKind.UInt: result = UInt; return true;
                case VariantTypeKind.UShort: result = UShort; return true;
            }

            result = default; return false;
        }

        public bool TryGetValue(out long result)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: result = Byte; return true;
                case VariantTypeKind.SByte: result = SByte; return true;
                case VariantTypeKind.Char: result = Char; return true;
                case VariantTypeKind.Int: result = Int; return true;
                case VariantTypeKind.UInt: result = UInt; return true;
                case VariantTypeKind.Long: result = Long; return true;
                case VariantTypeKind.Short: result = Short; return true;
                case VariantTypeKind.UShort: result = UShort; return true;
            }

            result = default; return false;
        }

        public bool TryGetValue(out ulong result)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: result = Byte; return true;
                case VariantTypeKind.Char: result = Char; return true;
                case VariantTypeKind.UInt: result = UInt; return true;
                case VariantTypeKind.ULong: result = ULong; return true;
                case VariantTypeKind.UShort: result = UShort; return true;
            }

            result = default; return false;
        }

        public bool TryGetValue(out short result)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: result = Byte; return true;
                case VariantTypeKind.SByte: result = SByte; return true;
                case VariantTypeKind.Short: result = Short; return true;
            }

            result = default; return false;
        }

        public bool TryGetValue(out ushort result)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: result = Byte; return true;
                case VariantTypeKind.Char: result = Char; return true;
                case VariantTypeKind.UShort: result = UShort; return true;
            }

            result = default; return false;
        }

        public bool TryGetValue(out string result)
        {
            if (TypeKind == VariantTypeKind.String && Object is string stringValue)
            {
                result = stringValue;
                return true;
            }

            result = default;
            return false;
        }

        public bool TryGetValue(out object result)
        {
            if (TypeKind == VariantTypeKind.Object && Object is object objectValue)
            {
                result = objectValue;
                return true;
            }

            result = default;
            return false;
        }

        public bool TrySetValueTo(ref bool dest)
        {
            if (TypeKind == VariantTypeKind.Bool)
            {
                dest = Bool; return true;
            }

            return false;
        }

        public bool TrySetValueTo(ref byte dest)
        {
            if (TypeKind == VariantTypeKind.Byte)
            {
                dest = Byte; return true;
            }

            return false;
        }

        public bool TrySetValueTo(ref sbyte dest)
        {
            if (TypeKind == VariantTypeKind.SByte)
            {
                dest = SByte; return true;
            }

            return false;
        }

        public bool TrySetValueTo(ref char dest)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.SByte: dest = (char)SByte; return true;
                case VariantTypeKind.Char: dest = Char; return true;
                case VariantTypeKind.UShort: dest = (char)UShort; return true;
            }

            return false;
        }

        public bool TrySetValueTo(ref double dest)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: dest = Byte; return true;
                case VariantTypeKind.SByte: dest = SByte; return true;
                case VariantTypeKind.Char: dest = Char; return true;
                case VariantTypeKind.Double: dest = Double; return true;
                case VariantTypeKind.Float: dest = Float; return true;
                case VariantTypeKind.Int: dest = Int; return true;
                case VariantTypeKind.UInt: dest = UInt; return true;
                case VariantTypeKind.Long: dest = Long; return true;
                case VariantTypeKind.ULong: dest = ULong; return true;
                case VariantTypeKind.Short: dest = Short; return true;
                case VariantTypeKind.UShort: dest = UShort; return true;
            }

            return false;
        }

        public bool TrySetValueTo(ref float dest)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: dest = Byte; return true;
                case VariantTypeKind.SByte: dest = SByte; return true;
                case VariantTypeKind.Char: dest = Char; return true;
                case VariantTypeKind.Float: dest = Float; return true;
                case VariantTypeKind.Int: dest = Int; return true;
                case VariantTypeKind.UInt: dest = UInt; return true;
                case VariantTypeKind.Long: dest = Long; return true;
                case VariantTypeKind.ULong: dest = ULong; return true;
                case VariantTypeKind.Short: dest = Short; return true;
                case VariantTypeKind.UShort: dest = UShort; return true;
            }

            return false;
        }

        public bool TrySetValueTo(ref int dest)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: dest = Byte; return true;
                case VariantTypeKind.SByte: dest = SByte; return true;
                case VariantTypeKind.Char: dest = Char; return true;
                case VariantTypeKind.Int: dest = Int; return true;
                case VariantTypeKind.Short: dest = Short; return true;
                case VariantTypeKind.UShort: dest = UShort; return true;
            }

            return false;
        }

        public bool TrySetValueTo(ref uint dest)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: dest = Byte; return true;
                case VariantTypeKind.Char: dest = Char; return true;
                case VariantTypeKind.UInt: dest = UInt; return true;
                case VariantTypeKind.UShort: dest = UShort; return true;
            }

            return false;
        }

        public bool TrySetValueTo(ref long dest)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: dest = Byte; return true;
                case VariantTypeKind.SByte: dest = SByte; return true;
                case VariantTypeKind.Char: dest = Char; return true;
                case VariantTypeKind.Int: dest = Int; return true;
                case VariantTypeKind.UInt: dest = UInt; return true;
                case VariantTypeKind.Long: dest = Long; return true;
                case VariantTypeKind.Short: dest = Short; return true;
                case VariantTypeKind.UShort: dest = UShort; return true;
            }

            return false;
        }

        public bool TrySetValueTo(ref ulong dest)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: dest = Byte; return true;
                case VariantTypeKind.Char: dest = Char; return true;
                case VariantTypeKind.UInt: dest = UInt; return true;
                case VariantTypeKind.ULong: dest = ULong; return true;
                case VariantTypeKind.UShort: dest = UShort; return true;
            }

            return false;
        }

        public bool TrySetValueTo(ref short dest)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: dest = Byte; return true;
                case VariantTypeKind.SByte: dest = SByte; return true;
                case VariantTypeKind.Short: dest = Short; return true;
            }

            return false;
        }

        public bool TrySetValueTo(ref ushort dest)
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Byte: dest = Byte; return true;
                case VariantTypeKind.Char: dest = Char; return true;
                case VariantTypeKind.UShort: dest = UShort; return true;
            }

            return false;
        }

        public bool TrySetValueTo(ref string dest)
        {
            if (TypeKind == VariantTypeKind.String && Object is string stringValue)
            {
                dest = stringValue;
                return true;
            }

            return false;
        }

        public bool TrySetValueTo(ref object dest)
        {
            if (TypeKind == VariantTypeKind.Object && Object is object objectValue)
            {
                dest = objectValue;
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            switch (TypeKind)
            {
                case VariantTypeKind.Bool: return Bool.ToString();
                case VariantTypeKind.Byte: return Byte.ToString();
                case VariantTypeKind.SByte: return SByte.ToString();
                case VariantTypeKind.Char: return Char.ToString();
                case VariantTypeKind.Double: return Double.ToString();
                case VariantTypeKind.Float: return Float.ToString();
                case VariantTypeKind.Int: return Int.ToString();
                case VariantTypeKind.UInt: return UInt.ToString();
                case VariantTypeKind.Long: return Long.ToString();
                case VariantTypeKind.ULong: return ULong.ToString();
                case VariantTypeKind.Short: return Short.ToString();
                case VariantTypeKind.UShort: return UShort.ToString();
                case VariantTypeKind.String: return Object is string stringVal ? stringVal : string.Empty;
                case VariantTypeKind.Object: return Object is object objectVal ? objectVal.ToString() : TypeId.ToType().ToString();
                case VariantTypeKind.ValueType: return VariantConverter.ToString(this);
                default:
                {
                    return (TypeId != TypeId.Undefined)
                        ? $"Undefined: {TypeId.ToType()}"
                        : string.Empty;
                }
            }
        }
    }
}
