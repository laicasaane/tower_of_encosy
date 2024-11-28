using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Modules.Unions;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Profiling;

namespace EncosyTower.Tests.Unions
{
    public class UnionTests
    {
        [Test, Performance]
        public void Union_Performance()
        {
            var i = 0;

            Measure.Method(() => {
                Profiler.BeginSample("Union");
                {
                    var u = new Union(i);
                    Process(u);
                    i++;
                }
                Profiler.EndSample();
            })
                .WarmupCount(1)
                .IterationsPerMeasurement(5000)
                .MeasurementCount(20)
                .Run();
        }

        [Test, Performance]
        public void UnionBig_Performance()
        {
            var i = 0;

            Measure.Method(() => {
                Profiler.BeginSample("UnionBig");
                {
                    var u = new UnionBig(i);
                    Process(u);
                    i++;
                }
                Profiler.EndSample();
            })
                .WarmupCount(1)
                .IterationsPerMeasurement(5000)
                .MeasurementCount(20)
                .Run();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Process(in Union v)
        {
            _ = v.Int;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Process(in UnionBig v)
        {
            _ = v.Int;
        }

        [StructLayout(LayoutKind.Explicit, Size = 9996)]
        private struct Big { }

        [StructLayout(LayoutKind.Explicit)]
        private readonly struct UnionBig
        {
            [FieldOffset(0)] public readonly UnionBase Base;
            [FieldOffset(0)] public readonly Big Big;
            [FieldOffset(0)] public readonly bool Bool;
            [FieldOffset(0)] public readonly byte Byte;
            [FieldOffset(0)] public readonly sbyte SByte;
            [FieldOffset(0)] public readonly char Char;
            [FieldOffset(0)] public readonly double Double;
            [FieldOffset(0)] public readonly float Float;
            [FieldOffset(0)] public readonly int Int;
            [FieldOffset(0)] public readonly uint UInt;
            [FieldOffset(0)] public readonly long Long;
            [FieldOffset(0)] public readonly ulong ULong;
            [FieldOffset(0)] public readonly short Short;
            [FieldOffset(0)] public readonly ushort UShort;

            public UnionBig(int intValue) : this()
            {
                Int = intValue;
            }
        }
    }
}
