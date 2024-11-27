#if UNITY_LOGGING

using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Logging;
using Unity.Mathematics;
using UnityEngine;

namespace EncosyTower.Modules.Logging
{
    [HideInStackTrace]
    partial class RuntimeLoggerAPI
    {
        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CallerInfo(CallerInfo callerInfo)
        {
            var fs = callerInfo.ToFixedString();
            Log.Info(fs);
        }

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CallerInfo(string message, CallerInfo callerInfo)
        {
            var fs = callerInfo.ToFixedString();
            Log.Info("{0} :: {1}", message, fs);
        }

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in FixedString32Bytes value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in FixedString32Bytes value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in FixedString32Bytes value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in FixedString64Bytes value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in FixedString64Bytes value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in FixedString64Bytes value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in FixedString128Bytes value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in FixedString128Bytes value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in FixedString128Bytes value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in FixedString512Bytes value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in FixedString512Bytes value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in FixedString512Bytes value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in FixedString4096Bytes value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in FixedString4096Bytes value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in FixedString4096Bytes value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(byte value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(byte value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(byte value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(sbyte value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(sbyte value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(sbyte value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(short value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(short value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(short value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(ushort value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(ushort value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(ushort value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(int value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(int value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(int value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(uint value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(uint value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(uint value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(long value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(long value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(long value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(ulong value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(ulong value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(ulong value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(float value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(float value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(float value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(double value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(double value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(double value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(Vector2 value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(Vector2 value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(Vector2 value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in Vector3 value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in Vector3 value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in Vector3 value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in Color value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in Color value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in Color value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(int2 value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(int2 value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(int2 value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in int3 value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in int3 value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in int3 value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in int4 value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in int4 value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in int4 value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(uint2 value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(uint2 value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(uint2 value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in uint3 value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in uint3 value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in uint3 value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in uint4 value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in uint4 value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in uint4 value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(float2 value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(float2 value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(float2 value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in float3 value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in float3 value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in float3 value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in float4 value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in float4 value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in float4 value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in double2 value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in double2 value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in double2 value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in double3 value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in double3 value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in double3 value)
            => Log.Error(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(in double4 value)
            => Log.Info(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in double4 value)
            => Log.Warning(value);

        [HideInStackTrace, HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in double4 value)
            => Log.Error(value);
    }
}

#endif
