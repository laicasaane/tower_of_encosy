#if UNITY_EDITOR && ANNULUS_CODEGEN && ENCOSY_STAT_VALUE_TYPES_GENERATOR

using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace EncosyTower.Entities.Stats.Generators
{
    internal static class GeneratorAPI
    {
        public const string AGGRESSIVE_INLINING = "[MethodImpl(MethodImplOptions.AggressiveInlining)]";
        public const string STRUCT_LAYOUT_EXPLICIT = "[StructLayout(LayoutKind.Explicit)]";
        public const string FIELD_OFFSET_0 = "[FieldOffset(0)]";
        public const string FIELD_OFFSET_1 = "[FieldOffset(1)]";
        public const string COMMON_FOLDER = "../Common";

        public static readonly string[] Types = new string[] {
            "bool",
            nameof(bool2),
            nameof(bool2x2),
            nameof(bool2x3),
            nameof(bool2x4),
            nameof(bool3),
            nameof(bool3x2),
            nameof(bool3x3),
            nameof(bool3x4),
            nameof(bool4),
            nameof(bool4x2),
            nameof(bool4x3),
            nameof(bool4x4),
            "byte",
            "double",
            nameof(double2),
            nameof(double2x2),
            nameof(double2x3),
            nameof(double2x4),
            nameof(double3),
            nameof(double3x2),
            nameof(double3x3),
            nameof(double3x4),
            nameof(double4),
            nameof(double4x2),
            nameof(double4x3),
            nameof(double4x4),
            "float",
            nameof(float2),
            nameof(float2x2),
            nameof(float2x3),
            nameof(float2x4),
            nameof(float3),
            nameof(float3x2),
            nameof(float3x3),
            nameof(float3x4),
            nameof(float4),
            nameof(float4x2),
            nameof(float4x3),
            nameof(float4x4),
            nameof(half),
            nameof(half2),
            nameof(half3),
            nameof(half4),
            "int",
            nameof(int2),
            nameof(int2x2),
            nameof(int2x3),
            nameof(int2x4),
            nameof(int3),
            nameof(int3x2),
            nameof(int3x3),
            nameof(int3x4),
            nameof(int4),
            nameof(int4x2),
            nameof(int4x3),
            nameof(int4x4),
            "long",
            "sbyte",
            "short",
            "uint",
            nameof(uint2),
            nameof(uint2x2),
            nameof(uint2x3),
            nameof(uint2x4),
            nameof(uint3),
            nameof(uint3x2),
            nameof(uint3x3),
            nameof(uint3x4),
            nameof(uint4),
            nameof(uint4x2),
            nameof(uint4x3),
            nameof(uint4x4),
            "ulong",
            "ushort",
        };

        public static readonly bool[] EqualOperators = new bool[] {
            true,  /// <see cref="bool"/>
            false, /// <see cref="bool2"/>
            false, /// <see cref="bool2x2"/>
            false, /// <see cref="bool2x2"/>
            false, /// <see cref="bool2x4"/>
            false, /// <see cref="bool3"/>
            false, /// <see cref="bool3x2"/>
            false, /// <see cref="bool3x2"/>
            false, /// <see cref="bool3x4"/>
            false, /// <see cref="bool4"/>
            false, /// <see cref="bool4x2"/>
            false, /// <see cref="bool4x2"/>
            false, /// <see cref="bool4x4"/>
            true,  /// <see cref="byte"/>
            true,  /// <see cref="double"/>
            false, /// <see cref="double2"/>
            false, /// <see cref="double2x2"/>
            false, /// <see cref="double2x2"/>
            false, /// <see cref="double2x4"/>
            false, /// <see cref="double3"/>
            false, /// <see cref="double3x2"/>
            false, /// <see cref="double3x2"/>
            false, /// <see cref="double3x4"/>
            false, /// <see cref="double4"/>
            false, /// <see cref="double4x2"/>
            false, /// <see cref="double4x2"/>
            false, /// <see cref="double4x4"/>
            true,  /// <see cref="float"/>
            false, /// <see cref="float2"/>
            false, /// <see cref="float2x2"/>
            false, /// <see cref="float2x2"/>
            false, /// <see cref="float2x4"/>
            false, /// <see cref="float3"/>
            false, /// <see cref="float3x2"/>
            false, /// <see cref="float3x2"/>
            false, /// <see cref="float3x4"/>
            false, /// <see cref="float4"/>
            false, /// <see cref="float4x2"/>
            false, /// <see cref="float4x2"/>
            false, /// <see cref="float4x4"/>
            true,  /// <see cref="half"/>
            false, /// <see cref="half2"/>
            false, /// <see cref="half3"/>
            false, /// <see cref="half4"/>
            true,  /// <see cref="int"/>
            false, /// <see cref="int2"/>
            false, /// <see cref="int2x2"/>
            false, /// <see cref="int2x2"/>
            false, /// <see cref="int2x4"/>
            false, /// <see cref="int3"/>
            false, /// <see cref="int3x2"/>
            false, /// <see cref="int3x2"/>
            false, /// <see cref="int3x4"/>
            false, /// <see cref="int4"/>
            false, /// <see cref="int4x2"/>
            false, /// <see cref="int4x2"/>
            false, /// <see cref="int4x4"/>
            true,  /// <see cref="long"/>
            true,  /// <see cref="sbyte"/>
            true,  /// <see cref="short"/>
            false, /// <see cref="uint"/>
            false, /// <see cref="uint2"/>
            false, /// <see cref="uint2x2"/>
            false, /// <see cref="uint2x2"/>
            false, /// <see cref="uint2x4"/>
            false, /// <see cref="uint3"/>
            false, /// <see cref="uint3x2"/>
            false, /// <see cref="uint3x2"/>
            false, /// <see cref="uint3x4"/>
            false, /// <see cref="uint4"/>
            false, /// <see cref="uint4x2"/>
            false, /// <see cref="uint4x2"/>
            false, /// <see cref="uint4x4"/>
            true,  /// <see cref="ulong"/>
            true,  /// <see cref="ushort"/>
        };

        public static readonly string[] TypeNames = new string[] {
            "Bool",
            "Bool2",
            "Bool2x2",
            "Bool2x3",
            "Bool2x4",
            "Bool3",
            "Bool3x2",
            "Bool3x3",
            "Bool3x4",
            "Bool4",
            "Bool4x2",
            "Bool4x3",
            "Bool4x4",
            "Byte",
            "Double",
            "Double2",
            "Double2x2",
            "Double2x3",
            "Double2x4",
            "Double3",
            "Double3x2",
            "Double3x3",
            "Double3x4",
            "Double4",
            "Double4x2",
            "Double4x3",
            "Double4x4",
            "Float",
            "Float2",
            "Float2x2",
            "Float2x3",
            "Float2x4",
            "Float3",
            "Float3x2",
            "Float3x3",
            "Float3x4",
            "Float4",
            "Float4x2",
            "Float4x3",
            "Float4x4",
            "Half",
            "Half2",
            "Half3",
            "Half4",
            "Int",
            "Int2",
            "Int2x2",
            "Int2x3",
            "Int2x4",
            "Int3",
            "Int3x2",
            "Int3x3",
            "Int3x4",
            "Int4",
            "Int4x2",
            "Int4x3",
            "Int4x4",
            "Long",
            "SByte",
            "Short",
            "UInt",
            "UInt2",
            "UInt2x2",
            "UInt2x3",
            "UInt2x4",
            "UInt3",
            "UInt3x2",
            "UInt3x3",
            "UInt3x4",
            "UInt4",
            "UInt4x2",
            "UInt4x3",
            "UInt4x4",
            "ULong",
            "UShort",
        };

        public static readonly string[] OneConstructors = new string[] {
            "true",
            "new bool2(true)",
            "new bool2x2(true)",
            "new bool2x3(true)",
            "new bool2x4(true)",
            "new bool3(true)",
            "new bool3x2(true)",
            "new bool3x3(true)",
            "new bool3x4(true)",
            "new bool4(true)",
            "new bool4x2(true)",
            "new bool4x3(true)",
            "new bool4x4(true)",
            "(byte)1",
            "(double)1.0",
            "new double2(1.0)",
            "new double2x2(1.0)",
            "new double2x3(1.0)",
            "new double2x4(1.0)",
            "new double3(1.0)",
            "new double3x2(1.0)",
            "new double3x3(1.0)",
            "new double3x4(1.0)",
            "new double4(1.0)",
            "new double4x2(1.0)",
            "new double4x3(1.0)",
            "new double4x4(1.0)",
            "(float)1f",
            "new float2(1f)",
            "new float2x2(1f)",
            "new float2x3(1f)",
            "new float2x4(1f)",
            "new float3(1f)",
            "new float3x2(1f)",
            "new float3x3(1f)",
            "new float3x4(1f)",
            "new float4(1f)",
            "new float4x2(1f)",
            "new float4x3(1f)",
            "new float4x4(1f)",
            "math.half(1f)",
            "math.half2(1f)",
            "math.half3(1f)",
            "math.half4(1f)",
            "(int)1",
            "new int2(1)",
            "new int2x2(1)",
            "new int2x3(1)",
            "new int2x4(1)",
            "new int3(1)",
            "new int3x2(1)",
            "new int3x3(1)",
            "new int3x4(1)",
            "new int4(1)",
            "new int4x2(1)",
            "new int4x3(1)",
            "new int4x4(1)",
            "(long)1",
            "(sbyte)1",
            "(short)1",
            "(uint)1",
            "new uint2((uint)1)",
            "new uint2x2((uint)1)",
            "new uint2x3((uint)1)",
            "new uint2x4((uint)1)",
            "new uint3((uint)1)",
            "new uint3x2((uint)1)",
            "new uint3x3((uint)1)",
            "new uint3x4((uint)1)",
            "new uint4((uint)1)",
            "new uint4x2((uint)1)",
            "new uint4x3((uint)1)",
            "new uint4x4((uint)1)",
            "(ulong)1",
            "(ushort)1",
        };

        public static readonly int[] Sizes = new int[] {
            UnsafeUtility.SizeOf<bool>(),
            UnsafeUtility.SizeOf<bool2>(),
            UnsafeUtility.SizeOf<bool2x2>(),
            UnsafeUtility.SizeOf<bool2x3>(),
            UnsafeUtility.SizeOf<bool2x4>(),
            UnsafeUtility.SizeOf<bool3>(),
            UnsafeUtility.SizeOf<bool3x2>(),
            UnsafeUtility.SizeOf<bool3x3>(),
            UnsafeUtility.SizeOf<bool3x4>(),
            UnsafeUtility.SizeOf<bool4>(),
            UnsafeUtility.SizeOf<bool4x2>(),
            UnsafeUtility.SizeOf<bool4x3>(),
            UnsafeUtility.SizeOf<bool4x4>(),
            UnsafeUtility.SizeOf<byte>(),
            UnsafeUtility.SizeOf<double>(),
            UnsafeUtility.SizeOf<double2>(),
            UnsafeUtility.SizeOf<double2x2>(),
            UnsafeUtility.SizeOf<double2x3>(),
            UnsafeUtility.SizeOf<double2x4>(),
            UnsafeUtility.SizeOf<double3>(),
            UnsafeUtility.SizeOf<double3x2>(),
            UnsafeUtility.SizeOf<double3x3>(),
            UnsafeUtility.SizeOf<double3x4>(),
            UnsafeUtility.SizeOf<double4>(),
            UnsafeUtility.SizeOf<double4x2>(),
            UnsafeUtility.SizeOf<double4x3>(),
            UnsafeUtility.SizeOf<double4x4>(),
            UnsafeUtility.SizeOf<float>(),
            UnsafeUtility.SizeOf<float2>(),
            UnsafeUtility.SizeOf<float2x2>(),
            UnsafeUtility.SizeOf<float2x3>(),
            UnsafeUtility.SizeOf<float2x4>(),
            UnsafeUtility.SizeOf<float3>(),
            UnsafeUtility.SizeOf<float3x2>(),
            UnsafeUtility.SizeOf<float3x3>(),
            UnsafeUtility.SizeOf<float3x4>(),
            UnsafeUtility.SizeOf<float4>(),
            UnsafeUtility.SizeOf<float4x2>(),
            UnsafeUtility.SizeOf<float4x3>(),
            UnsafeUtility.SizeOf<float4x4>(),
            UnsafeUtility.SizeOf<half>(),
            UnsafeUtility.SizeOf<half2>(),
            UnsafeUtility.SizeOf<half3>(),
            UnsafeUtility.SizeOf<half4>(),
            UnsafeUtility.SizeOf<int>(),
            UnsafeUtility.SizeOf<int2>(),
            UnsafeUtility.SizeOf<int2x2>(),
            UnsafeUtility.SizeOf<int2x3>(),
            UnsafeUtility.SizeOf<int2x4>(),
            UnsafeUtility.SizeOf<int3>(),
            UnsafeUtility.SizeOf<int3x2>(),
            UnsafeUtility.SizeOf<int3x3>(),
            UnsafeUtility.SizeOf<int3x4>(),
            UnsafeUtility.SizeOf<int4>(),
            UnsafeUtility.SizeOf<int4x2>(),
            UnsafeUtility.SizeOf<int4x3>(),
            UnsafeUtility.SizeOf<int4x4>(),
            UnsafeUtility.SizeOf<long>(),
            UnsafeUtility.SizeOf<sbyte>(),
            UnsafeUtility.SizeOf<short>(),
            UnsafeUtility.SizeOf<uint>(),
            UnsafeUtility.SizeOf<uint2>(),
            UnsafeUtility.SizeOf<uint2x2>(),
            UnsafeUtility.SizeOf<uint2x3>(),
            UnsafeUtility.SizeOf<uint2x4>(),
            UnsafeUtility.SizeOf<uint3>(),
            UnsafeUtility.SizeOf<uint3x2>(),
            UnsafeUtility.SizeOf<uint3x3>(),
            UnsafeUtility.SizeOf<uint3x4>(),
            UnsafeUtility.SizeOf<uint4>(),
            UnsafeUtility.SizeOf<uint4x2>(),
            UnsafeUtility.SizeOf<uint4x3>(),
            UnsafeUtility.SizeOf<uint4x4>(),
            UnsafeUtility.SizeOf<ulong>(),
            UnsafeUtility.SizeOf<ushort>(),
        };
    }
}

#endif
