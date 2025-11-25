using System.Collections.Generic;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    public static class StatGeneratorAPI
    {
        public const string NAMESPACE = "EncosyTower.Entities.Stats";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        public static readonly string[] Types = new string[] {
            "bool",
            "bool2",
            "bool2x2",
            "bool2x3",
            "bool2x4",
            "bool3",
            "bool3x2",
            "bool3x3",
            "bool3x4",
            "bool4",
            "bool4x2",
            "bool4x3",
            "bool4x4",
            "byte",
            "double",
            "double2",
            "double2x2",
            "double2x3",
            "double2x4",
            "double3",
            "double3x2",
            "double3x3",
            "double3x4",
            "double4",
            "double4x2",
            "double4x3",
            "double4x4",
            "float",
            "float2",
            "float2x2",
            "float2x3",
            "float2x4",
            "float3",
            "float3x2",
            "float3x3",
            "float3x4",
            "float4",
            "float4x2",
            "float4x3",
            "float4x4",
            "half",
            "half2",
            "half3",
            "half4",
            "int",
            "int2",
            "int2x2",
            "int2x3",
            "int2x4",
            "int3",
            "int3x2",
            "int3x3",
            "int3x4",
            "int4",
            "int4x2",
            "int4x3",
            "int4x4",
            "long",
            "sbyte",
            "short",
            "uint",
            "uint2",
            "uint2x2",
            "uint2x3",
            "uint2x4",
            "uint3",
            "uint3x2",
            "uint3x3",
            "uint3x4",
            "uint4",
            "uint4x2",
            "uint4x3",
            "uint4x4",
            "ulong",
            "ushort",
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

        public static readonly string[] Namespaces = new string[] {
            "",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "",
            "",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "",
            "",
            "",
            "",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "global::Unity.Mathematics",
            "",
            "",
        };

        public static readonly int[] Sizes = new int[] {
            1,   // bool
            2,   // bool2
            4,   // bool2x2
            6,   // bool2x3
            8,   // bool2x4
            3,   // bool3
            6,   // bool3x2
            9,   // bool3x3
            12,  // bool3x4
            4,   // bool4
            8,   // bool4x2
            12,  // bool4x3
            16,  // bool4x4
            1,   // byte
            8,   // double
            16,  // double2
            32,  // double2x2
            48,  // double2x3
            64,  // double2x4
            24,  // double3
            48,  // double3x2
            72,  // double3x3
            96,  // double3x4
            32,  // double4
            64,  // double4x2
            96,  // double4x3
            128, // double4x4
            4,   // float
            8,   // float2
            16,  // float2x2
            24,  // float2x3
            32,  // float2x4
            12,  // float3
            24,  // float3x2
            36,  // float3x3
            48,  // float3x4
            16,  // float4
            32,  // float4x2
            48,  // float4x3
            64,  // float4x4
            2,   // half
            4,   // half2
            6,   // half3
            8,   // half4
            4,   // int
            8,   // int2
            16,  // int2x2
            24,  // int2x3
            32,  // int2x4
            12,  // int3
            24,  // int3x2
            36,  // int3x3
            48,  // int3x4
            16,  // int4
            32,  // int4x2
            48,  // int4x3
            64,  // int4x4
            8,   // long
            1,   // sbyte
            2,   // short
            4,   // uint
            8,   // uint2
            16,  // uint2x2
            24,  // uint2x3
            32,  // uint2x4
            12,  // uint3
            24,  // uint3x2
            36,  // uint3x3
            48,  // uint3x4
            16,  // uint4
            32,  // uint4x2
            48,  // uint4x3
            64,  // uint4x4
            8,   // ulong
            2,   // ushort
        };
    }

    public enum StatVariantType : byte
    {
        /// <summary>
        /// <see cref="bool"/> | 1 byte.
        /// </summary>
        Bool = 0,

        /// <summary>
        /// <see cref="bool2"/> | 2 bytes.
        /// </summary>
        Bool2 = 1,

        /// <summary>
        /// <see cref="bool2x2"/> | 4 bytes.
        /// </summary>
        Bool2x2 = 2,

        /// <summary>
        /// <see cref="bool2x3"/> | 6 bytes.
        /// </summary>
        Bool2x3 = 3,

        /// <summary>
        /// <see cref="bool2x4"/> | 8 bytes.
        /// </summary>
        Bool2x4 = 4,

        /// <summary>
        /// <see cref="bool3"/> | 3 bytes.
        /// </summary>
        Bool3 = 5,

        /// <summary>
        /// <see cref="bool3x2"/> | 6 bytes.
        /// </summary>
        Bool3x2 = 6,

        /// <summary>
        /// <see cref="bool3x3"/> | 9 bytes.
        /// </summary>
        Bool3x3 = 7,

        /// <summary>
        /// <see cref="bool3x4"/> | 12 bytes.
        /// </summary>
        Bool3x4 = 8,

        /// <summary>
        /// <see cref="bool4"/> | 4 bytes.
        /// </summary>
        Bool4 = 9,

        /// <summary>
        /// <see cref="bool4x2"/> | 8 bytes.
        /// </summary>
        Bool4x2 = 10,

        /// <summary>
        /// <see cref="bool4x3"/> | 12 bytes.
        /// </summary>
        Bool4x3 = 11,

        /// <summary>
        /// <see cref="bool4x4"/> | 16 bytes.
        /// </summary>
        Bool4x4 = 12,

        /// <summary>
        /// <see cref="byte"/> | 1 byte.
        /// </summary>
        Byte = 13,

        /// <summary>
        /// <see cref="double"/> | 8 bytes.
        /// </summary>
        Double = 14,

        /// <summary>
        /// <see cref="double2"/> | 16 bytes.
        /// </summary>
        Double2 = 15,

        /// <summary>
        /// <see cref="double2x2"/> | 32 bytes.
        /// </summary>
        Double2x2 = 16,

        /// <summary>
        /// <see cref="double2x3"/> | 48 bytes.
        /// </summary>
        Double2x3 = 17,

        /// <summary>
        /// <see cref="double2x4"/> | 64 bytes.
        /// </summary>
        Double2x4 = 18,

        /// <summary>
        /// <see cref="double3"/> | 24 bytes.
        /// </summary>
        Double3 = 19,

        /// <summary>
        /// <see cref="double3x2"/> | 48 bytes.
        /// </summary>
        Double3x2 = 20,

        /// <summary>
        /// <see cref="double3x3"/> | 72 bytes.
        /// </summary>
        Double3x3 = 21,

        /// <summary>
        /// <see cref="double3x4"/> | 96 bytes.
        /// </summary>
        Double3x4 = 22,

        /// <summary>
        /// <see cref="double4"/> | 32 bytes.
        /// </summary>
        Double4 = 23,

        /// <summary>
        /// <see cref="double4x2"/> | 64 bytes.
        /// </summary>
        Double4x2 = 24,

        /// <summary>
        /// <see cref="double4x3"/> | 96 bytes.
        /// </summary>
        Double4x3 = 25,

        /// <summary>
        /// <see cref="double4x4"/> | 128 bytes.
        /// </summary>
        Double4x4 = 26,

        /// <summary>
        /// <see cref="float"/> | 4 bytes.
        /// </summary>
        Float = 27,

        /// <summary>
        /// <see cref="float2"/> | 8 bytes.
        /// </summary>
        Float2 = 28,

        /// <summary>
        /// <see cref="float2x2"/> | 16 bytes.
        /// </summary>
        Float2x2 = 29,

        /// <summary>
        /// <see cref="float2x3"/> | 24 bytes.
        /// </summary>
        Float2x3 = 30,

        /// <summary>
        /// <see cref="float2x4"/> | 32 bytes.
        /// </summary>
        Float2x4 = 31,

        /// <summary>
        /// <see cref="float3"/> | 12 bytes.
        /// </summary>
        Float3 = 32,

        /// <summary>
        /// <see cref="float3x2"/> | 24 bytes.
        /// </summary>
        Float3x2 = 33,

        /// <summary>
        /// <see cref="float3x3"/> | 36 bytes.
        /// </summary>
        Float3x3 = 34,

        /// <summary>
        /// <see cref="float3x4"/> | 48 bytes.
        /// </summary>
        Float3x4 = 35,

        /// <summary>
        /// <see cref="float4"/> | 16 bytes.
        /// </summary>
        Float4 = 36,

        /// <summary>
        /// <see cref="float4x2"/> | 32 bytes.
        /// </summary>
        Float4x2 = 37,

        /// <summary>
        /// <see cref="float4x3"/> | 48 bytes.
        /// </summary>
        Float4x3 = 38,

        /// <summary>
        /// <see cref="float4x4"/> | 64 bytes.
        /// </summary>
        Float4x4 = 39,

        /// <summary>
        /// <see cref="half"/> | 2 bytes.
        /// </summary>
        Half = 40,

        /// <summary>
        /// <see cref="half2"/> | 4 bytes.
        /// </summary>
        Half2 = 41,

        /// <summary>
        /// <see cref="half3"/> | 6 bytes.
        /// </summary>
        Half3 = 42,

        /// <summary>
        /// <see cref="half4"/> | 8 bytes.
        /// </summary>
        Half4 = 43,

        /// <summary>
        /// <see cref="int"/> | 4 bytes.
        /// </summary>
        Int = 44,

        /// <summary>
        /// <see cref="int2"/> | 8 bytes.
        /// </summary>
        Int2 = 45,

        /// <summary>
        /// <see cref="int2x2"/> | 16 bytes.
        /// </summary>
        Int2x2 = 46,

        /// <summary>
        /// <see cref="int2x3"/> | 24 bytes.
        /// </summary>
        Int2x3 = 47,

        /// <summary>
        /// <see cref="int2x4"/> | 32 bytes.
        /// </summary>
        Int2x4 = 48,

        /// <summary>
        /// <see cref="int3"/> | 12 bytes.
        /// </summary>
        Int3 = 49,

        /// <summary>
        /// <see cref="int3x2"/> | 24 bytes.
        /// </summary>
        Int3x2 = 50,

        /// <summary>
        /// <see cref="int3x3"/> | 36 bytes.
        /// </summary>
        Int3x3 = 51,

        /// <summary>
        /// <see cref="int3x4"/> | 48 bytes.
        /// </summary>
        Int3x4 = 52,

        /// <summary>
        /// <see cref="int4"/> | 16 bytes.
        /// </summary>
        Int4 = 53,

        /// <summary>
        /// <see cref="int4x2"/> | 32 bytes.
        /// </summary>
        Int4x2 = 54,

        /// <summary>
        /// <see cref="int4x3"/> | 48 bytes.
        /// </summary>
        Int4x3 = 55,

        /// <summary>
        /// <see cref="int4x4"/> | 64 bytes.
        /// </summary>
        Int4x4 = 56,

        /// <summary>
        /// <see cref="long"/> | 8 bytes.
        /// </summary>
        Long = 57,

        /// <summary>
        /// <see cref="sbyte"/> | 1 byte.
        /// </summary>
        SByte = 58,

        /// <summary>
        /// <see cref="short"/> | 2 bytes.
        /// </summary>
        Short = 59,

        /// <summary>
        /// <see cref="uint"/> | 4 bytes.
        /// </summary>
        UInt = 60,

        /// <summary>
        /// <see cref="uint2"/> | 8 bytes.
        /// </summary>
        UInt2 = 61,

        /// <summary>
        /// <see cref="uint2x2"/> | 16 bytes.
        /// </summary>
        UInt2x2 = 62,

        /// <summary>
        /// <see cref="uint2x3"/> | 24 bytes.
        /// </summary>
        UInt2x3 = 63,

        /// <summary>
        /// <see cref="uint2x4"/> | 32 bytes.
        /// </summary>
        UInt2x4 = 64,

        /// <summary>
        /// <see cref="uint3"/> | 12 bytes.
        /// </summary>
        UInt3 = 65,

        /// <summary>
        /// <see cref="uint3x2"/> | 24 bytes.
        /// </summary>
        UInt3x2 = 66,

        /// <summary>
        /// <see cref="uint3x3"/> | 36 bytes.
        /// </summary>
        UInt3x3 = 67,

        /// <summary>
        /// <see cref="uint3x4"/> | 48 bytes.
        /// </summary>
        UInt3x4 = 68,

        /// <summary>
        /// <see cref="uint4"/> | 16 bytes.
        /// </summary>
        UInt4 = 69,

        /// <summary>
        /// <see cref="uint4x2"/> | 32 bytes.
        /// </summary>
        UInt4x2 = 70,

        /// <summary>
        /// <see cref="uint4x3"/> | 48 bytes.
        /// </summary>
        UInt4x3 = 71,

        /// <summary>
        /// <see cref="uint4x4"/> | 64 bytes.
        /// </summary>
        UInt4x4 = 72,

        /// <summary>
        /// <see cref="ulong"/> | 8 bytes.
        /// </summary>
        ULong = 73,

        /// <summary>
        /// <see cref="ushort"/> | 2 bytes.
        /// </summary>
        UShort = 74,

    }
}
