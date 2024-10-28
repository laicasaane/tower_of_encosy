#if UNITY_MATHEMATICS

using System.Runtime.CompilerServices;

#pragma warning disable IDE1006

namespace Unity.Mathematics
{
    public static partial class umath
    {
        /// <summary>Returns the bit pattern of a ushort as a short.</summary>
        /// <param name="v">The ushort bits to copy.</param>
        /// <returns>The short with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short asshort(ushort v) => (short)v;

        /// <summary>Returns the bit pattern of a ushort2 as a short2.</summary>
        /// <param name="v">The ushort2 bits to copy.</param>
        /// <returns>The short2 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short2 asshort(ushort2 v) => new((short)v.x, (short)v.y);

        /// <summary>Returns the bit pattern of a ushort3 as a short3.</summary>
        /// <param name="v">The ushort3 bits to copy.</param>
        /// <returns>The short3 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short3 asshort(ushort3 v) => new((short)v.x, (short)v.y, (short)v.z);

        /// <summary>Returns the bit pattern of a ushort4 as a short4.</summary>
        /// <param name="v">The ushort4 bits to copy.</param>
        /// <returns>The short4 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short4 asshort(ushort4 v) => new((short)v.x, (short)v.y, (short)v.z, (short)v.w);

        /// <summary>Returns the bit pattern of an int as a short.</summary>
        /// <param name="v">The int bits to copy.</param>
        /// <returns>The short with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short asshort(int v) => (short)v;

        /// <summary>Returns the bit pattern of an int2 as a short2.</summary>
        /// <param name="v">The int2 bits to copy.</param>
        /// <returns>The short2 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short2 asshort(int2 v) => new((short)v.x, (short)v.y);

        /// <summary>Returns the bit pattern of an int3 as a short3.</summary>
        /// <param name="v">The int3 bits to copy.</param>
        /// <returns>The short3 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short3 asshort(int3 v) => new((short)v.x, (short)v.y, (short)v.z);

        /// <summary>Returns the bit pattern of an int4 as a short4.</summary>
        /// <param name="v">The int4 bits to copy.</param>
        /// <returns>The short4 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short4 asshort(int4 v) => new((short)v.x, (short)v.y, (short)v.z, (short)v.w);

        /// <summary>Returns the bit pattern of a uint as a short.</summary>
        /// <param name="v">The uint bits to copy.</param>
        /// <returns>The short with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short asshort(uint v) => (short)v;

        /// <summary>Returns the bit pattern of a uint2 as a short2.</summary>
        /// <param name="v">The uint2 bits to copy.</param>
        /// <returns>The short2 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short2 asshort(uint2 v) => new((short)v.x, (short)v.y);

        /// <summary>Returns the bit pattern of a uint3 as a short3.</summary>
        /// <param name="v">The uint3 bits to copy.</param>
        /// <returns>The short3 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short3 asshort(uint3 v) => new((short)v.x, (short)v.y, (short)v.z);

        /// <summary>Returns the bit pattern of a uint4 as a short4.</summary>
        /// <param name="v">The uint4 bits to copy.</param>
        /// <returns>The short4 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short4 asshort(uint4 v) => new((short)v.x, (short)v.y, (short)v.z, (short)v.w);

        /// <summary>Returns the bit pattern of a float as a short.</summary>
        /// <param name="v">The float bits to copy.</param>
        /// <returns>The short with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short asshort(float v) => (short)v;

        /// <summary>Returns the bit pattern of a float2 as a short2.</summary>
        /// <param name="v">The float2 bits to copy.</param>
        /// <returns>The short2 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short2 asshort(float2 v) => new((short)v.x, (short)v.y);

        /// <summary>Returns the bit pattern of a float3 as a short3.</summary>
        /// <param name="v">The float3 bits to copy.</param>
        /// <returns>The short3 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short3 asshort(float3 v) => new((short)v.x, (short)v.y, (short)v.z);

        /// <summary>Returns the bit pattern of a float4 as a short4.</summary>
        /// <param name="v">The float4 bits to copy.</param>
        /// <returns>The short4 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short4 asshort(float4 v) => new((short)v.x, (short)v.y, (short)v.z, (short)v.w);

        /// <summary>Returns the bit pattern of a short as a ushort.</summary>
        /// <param name="v">The short bits to copy.</param>
        /// <returns>The ushort with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort asushort(short v) => (ushort)v;

        /// <summary>Returns the bit pattern of a short2 as a ushort2.</summary>
        /// <param name="v">The short2 bits to copy.</param>
        /// <returns>The ushort2 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort2 asushort(short2 v) => new((ushort)v.x, (ushort)v.y);

        /// <summary>Returns the bit pattern of a short3 as a ushort3.</summary>
        /// <param name="v">The short3 bits to copy.</param>
        /// <returns>The ushort3 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort3 asushort(short3 v) => new((ushort)v.x, (ushort)v.y, (ushort)v.z);

        /// <summary>Returns the bit pattern of a short4 as a ushort4.</summary>
        /// <param name="v">The short4 bits to copy.</param>
        /// <returns>The ushort4 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort4 asushort(short4 v) => new((ushort)v.x, (ushort)v.y, (ushort)v.z, (ushort)v.w);

        /// <summary>Returns the bit pattern of an int as a ushort.</summary>
        /// <param name="v">The int bits to copy.</param>
        /// <returns>The ushort with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort asushort(int v) => (ushort)v;

        /// <summary>Returns the bit pattern of an int2 as a ushort2.</summary>
        /// <param name="v">The int2 bits to copy.</param>
        /// <returns>The ushort2 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort2 asushort(int2 v) => new((ushort)v.x, (ushort)v.y);

        /// <summary>Returns the bit pattern of an int3 as a ushort3.</summary>
        /// <param name="v">The int3 bits to copy.</param>
        /// <returns>The ushort3 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort3 asushort(int3 v) => new((ushort)v.x, (ushort)v.y, (ushort)v.z);

        /// <summary>Returns the bit pattern of an int4 as a ushort4.</summary>
        /// <param name="v">The int4 bits to copy.</param>
        /// <returns>The ushort4 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort4 asushort(int4 v) => new((ushort)v.x, (ushort)v.y, (ushort)v.z, (ushort)v.w);

        /// <summary>Returns the bit pattern of a uint as a ushort.</summary>
        /// <param name="v">The uint bits to copy.</param>
        /// <returns>The ushort with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort asushort(uint v) => (ushort)v;

        /// <summary>Returns the bit pattern of a uint2 as a ushort2.</summary>
        /// <param name="v">The uint2 bits to copy.</param>
        /// <returns>The ushort2 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort2 asushort(uint2 v) => new((ushort)v.x, (ushort)v.y);

        /// <summary>Returns the bit pattern of a uint3 as a ushort3.</summary>
        /// <param name="v">The uint3 bits to copy.</param>
        /// <returns>The ushort3 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort3 asushort(uint3 v) => new((ushort)v.x, (ushort)v.y, (ushort)v.z);

        /// <summary>Returns the bit pattern of a uint4 as a ushort4.</summary>
        /// <param name="v">The uint4 bits to copy.</param>
        /// <returns>The ushort4 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort4 asushort(uint4 v) => new((ushort)v.x, (ushort)v.y, (ushort)v.z, (ushort)v.w);

        /// <summary>Returns the bit pattern of a float as a ushort.</summary>
        /// <param name="v">The float bits to copy.</param>
        /// <returns>The ushort with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort asushort(float v) => (ushort)v;

        /// <summary>Returns the bit pattern of a float2 as a ushort2.</summary>
        /// <param name="v">The float2 bits to copy.</param>
        /// <returns>The ushort2 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort2 asushort(float2 v) => new((ushort)v.x, (ushort)v.y);

        /// <summary>Returns the bit pattern of a float3 as a ushort3.</summary>
        /// <param name="v">The float3 bits to copy.</param>
        /// <returns>The ushort3 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort3 asushort(float3 v) => new((ushort)v.x, (ushort)v.y, (ushort)v.z);

        /// <summary>Returns the bit pattern of a float4 as a ushort4.</summary>
        /// <param name="v">The float4 bits to copy.</param>
        /// <returns>The ushort4 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort4 asushort(float4 v) => new((ushort)v.x, (ushort)v.y, (ushort)v.z, (ushort)v.w);

        /// <summary>Returns the bit pattern of a short as an int.</summary>
        /// <param name="v">The short bits to copy.</param>
        /// <returns>The int with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int asint(short v) => v;

        /// <summary>Returns the bit pattern of a short2 as an int2.</summary>
        /// <param name="v">The short2 bits to copy.</param>
        /// <returns>The int2 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 asint(short2 v) => new(v.x, v.y);

        /// <summary>Returns the bit pattern of a short3 as an int3.</summary>
        /// <param name="v">The short3 bits to copy.</param>
        /// <returns>The int3 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 asint(short3 v) => new(v.x, v.y, v.z);

        /// <summary>Returns the bit pattern of a short4 as an int4.</summary>
        /// <param name="v">The short4 bits to copy.</param>
        /// <returns>The int4 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 asint(short4 v) => new(v.x, v.y, v.z, v.w);

        /// <summary>Returns the bit pattern of a ushort as an int.</summary>
        /// <param name="v">The ushort bits to copy.</param>
        /// <returns>The int with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int asint(ushort v) => v;

        /// <summary>Returns the bit pattern of a ushort2 as an int2.</summary>
        /// <param name="v">The ushort2 bits to copy.</param>
        /// <returns>The int2 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 asint(ushort2 v) => new(v.x, v.y);

        /// <summary>Returns the bit pattern of a ushort3 as an int3.</summary>
        /// <param name="v">The ushort3 bits to copy.</param>
        /// <returns>The int3 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 asint(ushort3 v) => new(v.x, v.y, v.z);

        /// <summary>Returns the bit pattern of a ushort4 as an int4.</summary>
        /// <param name="v">The ushort4 bits to copy.</param>
        /// <returns>The int4 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 asint(ushort4 v) => new(v.x, v.y, v.z, v.w);

        /// <summary>Returns the bit pattern of an short as a uint.</summary>
        /// <param name="v">The short bits to copy.</param>
        /// <returns>The uint with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint asuint(short v) => (uint)v;

        /// <summary>Returns the bit pattern of an short2 as a uint2.</summary>
        /// <param name="v">The short2 bits to copy.</param>
        /// <returns>The uint2 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint2 asuint(short2 v) => new((uint)v.x, (uint)v.y);

        /// <summary>Returns the bit pattern of an short3 as a uint3.</summary>
        /// <param name="v">The short3 bits to copy.</param>
        /// <returns>The uint3 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint3 asuint(short3 v) => new((uint)v.x, (uint)v.y, (uint)v.z);

        /// <summary>Returns the bit pattern of an short4 as a uint4.</summary>
        /// <param name="v">The short4 bits to copy.</param>
        /// <returns>The uint4 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 asuint(short4 v) => new((uint)v.x, (uint)v.y, (uint)v.z, (uint)v.w);

        /// <summary>Returns the bit pattern of an ushort as a uint.</summary>
        /// <param name="v">The ushort bits to copy.</param>
        /// <returns>The uint with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint asuint(ushort v) => v;

        /// <summary>Returns the bit pattern of an ushort2 as a uint2.</summary>
        /// <param name="v">The ushort2 bits to copy.</param>
        /// <returns>The uint2 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint2 asuint(ushort2 v) => new(v.x, v.y);

        /// <summary>Returns the bit pattern of an ushort3 as a uint3.</summary>
        /// <param name="v">The ushort3 bits to copy.</param>
        /// <returns>The uint3 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint3 asuint(ushort3 v) => new(v.x, v.y, v.z);

        /// <summary>Returns the bit pattern of an ushort4 as a uint4.</summary>
        /// <param name="v">The ushort4 bits to copy.</param>
        /// <returns>The float4 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 asuint(ushort4 v) => new(v.x, v.y, v.z, v.w);

        /// <summary>Returns the bit pattern of an short as a float.</summary>
        /// <param name="v">The short bits to copy.</param>
        /// <returns>The float with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float asfloat(short v) => v;

        /// <summary>Returns the bit pattern of an short2 as a float2.</summary>
        /// <param name="v">The short2 bits to copy.</param>
        /// <returns>The float2 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 asfloat(short2 v) => new(v.x, v.y);

        /// <summary>Returns the bit pattern of an short3 as a float3.</summary>
        /// <param name="v">The short3 bits to copy.</param>
        /// <returns>The float3 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 asfloat(short3 v) => new(v.x, v.y, v.z);

        /// <summary>Returns the bit pattern of an short4 as a float4.</summary>
        /// <param name="v">The short4 bits to copy.</param>
        /// <returns>The float4 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 asfloat(short4 v) => new(v.x, v.y, v.z, v.w);

        /// <summary>Returns the bit pattern of an ushort as a float.</summary>
        /// <param name="v">The ushort bits to copy.</param>
        /// <returns>The float with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float asfloat(ushort v) => v;

        /// <summary>Returns the bit pattern of an ushort2 as a float2.</summary>
        /// <param name="v">The ushort2 bits to copy.</param>
        /// <returns>The float2 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 asfloat(ushort2 v) => new(v.x, v.y);

        /// <summary>Returns the bit pattern of an ushort3 as a float3.</summary>
        /// <param name="v">The ushort3 bits to copy.</param>
        /// <returns>The float3 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 asfloat(ushort3 v) => new(v.x, v.y, v.z);

        /// <summary>Returns the bit pattern of an ushort4 as a float4.</summary>
        /// <param name="v">The ushort4 bits to copy.</param>
        /// <returns>The float4 with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 asfloat(ushort4 v) => new(v.x, v.y, v.z, v.w);

        /// <summary>Returns trueValue if test is true, falseValue otherwise.</summary>
        /// <param name="falseValue">Value to use if test is false.</param>
        /// <param name="trueValue">Value to use if test is true.</param>
        /// <param name="test">Bool value to choose between falseValue and trueValue.</param>
        /// <returns>The selection between falseValue and trueValue according to bool test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T select<T>(T falseValue, T trueValue, bool test) where T : unmanaged => test ? trueValue : falseValue;

        /// <summary>Returns trueValue if test is true, falseValue otherwise.</summary>
        /// <param name="falseValue">Value to use if test is false.</param>
        /// <param name="trueValue">Value to use if test is true.</param>
        /// <param name="test">Bool value to choose between falseValue and trueValue.</param>
        /// <returns>The selection between falseValue and trueValue according to bool test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short select(short falseValue, short trueValue, bool test) => test ? trueValue : falseValue;

        /// <summary>Returns trueValue if test is true, falseValue otherwise.</summary>
        /// <param name="falseValue">Value to use if test is false.</param>
        /// <param name="trueValue">Value to use if test is true.</param>
        /// <param name="test">Bool value to choose between falseValue and trueValue.</param>
        /// <returns>The selection between falseValue and trueValue according to bool test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short2 select(short2 falseValue, short2 trueValue, bool test) => test ? trueValue : falseValue;

        /// <summary>Returns trueValue if test is true, falseValue otherwise.</summary>
        /// <param name="falseValue">Value to use if test is false.</param>
        /// <param name="trueValue">Value to use if test is true.</param>
        /// <param name="test">Bool value to choose between falseValue and trueValue.</param>
        /// <returns>The selection between falseValue and trueValue according to bool test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short3 select(short3 falseValue, short3 trueValue, bool test) => test ? trueValue : falseValue;

        /// <summary>Returns trueValue if test is true, falseValue otherwise.</summary>
        /// <param name="falseValue">Value to use if test is false.</param>
        /// <param name="trueValue">Value to use if test is true.</param>
        /// <param name="test">Bool value to choose between falseValue and trueValue.</param>
        /// <returns>The selection between falseValue and trueValue according to bool test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short4 select(short4 falseValue, short4 trueValue, bool test) => test ? trueValue : falseValue;

        /// <summary>Returns trueValue if test is true, falseValue otherwise.</summary>
        /// <param name="falseValue">Value to use if test is false.</param>
        /// <param name="trueValue">Value to use if test is true.</param>
        /// <param name="test">Bool value to choose between falseValue and trueValue.</param>
        /// <returns>The selection between falseValue and trueValue according to bool test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort select(ushort falseValue, ushort trueValue, bool test) => test ? trueValue : falseValue;

        /// <summary>Returns trueValue if test is true, falseValue otherwise.</summary>
        /// <param name="falseValue">Value to use if test is false.</param>
        /// <param name="trueValue">Value to use if test is true.</param>
        /// <param name="test">Bool value to choose between falseValue and trueValue.</param>
        /// <returns>The selection between falseValue and trueValue according to bool test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort2 select(ushort2 falseValue, ushort2 trueValue, bool test) => test ? trueValue : falseValue;

        /// <summary>Returns trueValue if test is true, falseValue otherwise.</summary>
        /// <param name="falseValue">Value to use if test is false.</param>
        /// <param name="trueValue">Value to use if test is true.</param>
        /// <param name="test">Bool value to choose between falseValue and trueValue.</param>
        /// <returns>The selection between falseValue and trueValue according to bool test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort3 select(ushort3 falseValue, ushort3 trueValue, bool test) => test ? trueValue : falseValue;

        /// <summary>Returns trueValue if test is true, falseValue otherwise.</summary>
        /// <param name="falseValue">Value to use if test is false.</param>
        /// <param name="trueValue">Value to use if test is true.</param>
        /// <param name="test">Bool value to choose between falseValue and trueValue.</param>
        /// <returns>The selection between falseValue and trueValue according to bool test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort4 select(ushort4 falseValue, ushort4 trueValue, bool test) => test ? trueValue : falseValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short2 swapx(this short2 v, short x) => new(x, v.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short2 swapy(this short2 v, short y) => new(v.x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short3 swapx(this short3 v, short x) => new(x, v.y, v.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short3 swapy(this short3 v, short y) => new(v.x, y, v.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short3 swapz(this short3 v, short z) => new(v.x, v.y, z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short4 swapx(this short4 v, short x) => new(x, v.y, v.z, v.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short4 swapy(this short4 v, short y) => new(v.x, y, v.z, v.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short4 swapz(this short4 v, short z) => new(v.x, v.y, z, v.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short4 swapw(this short4 v, short w) => new(v.x, v.y, v.z, w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort2 swapx(this ushort2 v, ushort x) => new(x, v.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort2 swapy(this ushort2 v, ushort y) => new(v.x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort3 swapx(this ushort3 v, ushort x) => new(x, v.y, v.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort3 swapy(this ushort3 v, ushort y) => new(v.x, y, v.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort3 swapz(this ushort3 v, ushort z) => new(v.x, v.y, z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort4 swapx(this ushort4 v, ushort x) => new(x, v.y, v.z, v.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort4 swapy(this ushort4 v, ushort y) => new(v.x, y, v.z, v.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort4 swapz(this ushort4 v, ushort z) => new(v.x, v.y, z, v.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort4 swapw(this ushort4 v, ushort w) => new(v.x, v.y, v.z, w);
    }
}

#endif
